using UnityEngine;
using System.Xml;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Room : MonoBehaviour
{
    #region Direction declarations
    public enum Direction
    {
        Right,
        Forward,
        Left,
        Backward
    }

    static protected readonly Direction[] s_allDirections = new Direction[4]
    {
        Direction.Right,
        Direction.Forward,
        Direction.Left,
        Direction.Backward
    };
    #endregion

    [SerializeField] protected Vector3 m_size = new Vector3(5, 3, 5);
    [SerializeField, HideInInspector] protected RoomSkin m_skin;
    [SerializeField, HideInInspector] protected WallsDictionary m_wallsDictionary = new WallsDictionary();
    [SerializeField, HideInInspector] protected Transform m_roomStructure;

    protected List<Transform> m_watchedObjects = new List<Transform>();
    protected List<Transform> m_objectsInsideRoom = new List<Transform>();
    protected List<Transform> m_objectsOnEnterRoom = new List<Transform>();
    protected List<Transform> m_objectsOnLeaveRoom = new List<Transform>();

    virtual protected void Update()
    {
        CheckObjectsInRoom();
    }

    #region Configuration
    virtual public void Configure()
    {
        ConfigureRoomStructure();
        //ConfigureRoomCollider();
        ConfigureWalls();
        ConfigureFloor();
        ConfigureCeiling();

        Direction relationToLinkedRoom;
        Room linkedRoom = GetLinkedRoom(out relationToLinkedRoom);
        if(linkedRoom != null)
        {
            ConfigurePosition(linkedRoom, ReciprocalDirection(relationToLinkedRoom));
            ConfigureDoors(linkedRoom, relationToLinkedRoom, m_skin.doorSize);
        }
        else transform.position = Vector3.zero;

        gameObject.isStatic = true;
    }

    virtual protected void ConfigureRoomStructure()
    {
        m_roomStructure = new GameObject("Room structure").transform;
        m_roomStructure.SetParent(transform);
        m_roomStructure.localPosition = Vector3.zero;
    }

    /*
    virtual protected void ConfigureRoomCollider()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider == null) collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = m_size;
        collider.center = Vector3.up * m_size.y / 2;
    }
    */

    virtual protected void ConfigureWalls()
    {
        foreach(Direction direction in s_allDirections)
        {
            ConfigureWall(direction);
        }
    }

    virtual protected void ConfigureWall(Direction direction)
    {
        Vector3 directionVector = DirectionToVector(direction);
        float width = DirectionScale(direction, m_size).magnitude;
        Vector2 wallSize = new Vector2(width, m_size.y);
        string wallName = string.Format("Wall_{0}", direction);

        RoomParts.WallPart wall = Instantiate(m_skin.wall);
        wall.Configure(m_roomStructure, wallName, wallSize);

        wall.transform.localPosition = Vector3.Scale(m_size, directionVector) / 2;
        wall.transform.Rotate(Vector3.up, DirectionAngle(direction) - 90, Space.World);

        m_wallsDictionary.dictionary.Add(direction, wall);
    }

    virtual protected void ConfigureDoors(Room linkedRoom, Direction relationToLinkedRoom, Vector2 doorSize)
    {
        GameObject previousWall = m_wallsDictionary.dictionary[relationToLinkedRoom].gameObject;
        Transform roomStructure = previousWall.transform.parent;
        DestroyImmediate(previousWall);

        float width = DirectionScale(relationToLinkedRoom, m_size).magnitude;
        Vector2 wallSize = new Vector2(width, m_size.y);

        RoomParts.WallDoorPart newWall = Instantiate(m_skin.wallWithDoor);

        newWall.Configure(roomStructure, string.Format("Wall_{0}", relationToLinkedRoom), wallSize, doorSize, linkedRoom == null);

        newWall.transform.localPosition = Vector3.Scale(m_size, DirectionToVector(relationToLinkedRoom)) / 2;
        newWall.transform.Rotate(Vector3.up, DirectionAngle(relationToLinkedRoom) - 90, Space.World);

        m_wallsDictionary.dictionary[relationToLinkedRoom] = newWall;

        if (linkedRoom != null)
        {
            linkedRoom.ConfigureDoors(null, ReciprocalDirection(relationToLinkedRoom), doorSize);
        }
    }

    virtual protected void ConfigureFloor()
    {
        RoomParts.RoomPart floor = Instantiate(m_skin.floor);

        Vector2 floorSize = new Vector2(m_size.x, m_size.z);
        floor.Configure(m_roomStructure, "Floor", floorSize);

        floor.transform.localPosition = new Vector3(0, 0, 0);
    }

    virtual protected void ConfigureCeiling()
    {
        RoomParts.RoomPart ceiling = Instantiate(skin.ceiling);

        Vector2 ceilingSize = new Vector2(m_size.x, m_size.z);
        ceiling.Configure(m_roomStructure, "Ceiling", ceilingSize);

        ceiling.transform.localPosition = new Vector3(0, m_size.y, 0);
    }

    virtual protected void ConfigurePosition(Room linkedRoom, Direction relationToLinkedRoom)
    {
        Vector3 direction = DirectionToVector(relationToLinkedRoom);
        Vector3 position = linkedRoom.transform.position;
        position += Vector3.Scale(direction, m_size / 2);
        position += Vector3.Scale(direction, linkedRoom.m_size / 2);
        transform.position = position;
    }

    private Room GetLinkedRoom(out Direction relationToLinkedRoom)
    {
        Dictionary<Direction, Room> linkedRoomsRelationsDictionary = RoomManager.GetLinkedRooms(this);
        if(linkedRoomsRelationsDictionary.Count == 0)
        {
            relationToLinkedRoom = Direction.Right;
            return null;
        }

        relationToLinkedRoom = new List<Direction>(linkedRoomsRelationsDictionary.Keys)[0];
        return linkedRoomsRelationsDictionary[relationToLinkedRoom];
    }
    #endregion

    #region Trigger methods helpers
    virtual public void CheckObjectsInRoom()
    {
        for (int i = 0; i < m_watchedObjects.Count; i++)
        {
            Transform watchedObject = m_watchedObjects[i];
            if (ObjectIsInsideRoom(watchedObject))
            {
                if(!m_objectsInsideRoom.Contains(watchedObject))
                {
                    m_objectsOnEnterRoom.Add(watchedObject);
                    m_objectsInsideRoom.Add(watchedObject);
                }
                else if(m_objectsInsideRoom.Contains(watchedObject))
                {
                    m_objectsOnEnterRoom.Remove(watchedObject);
                }
            }
            else 
            {
                if (m_objectsInsideRoom.Contains(watchedObject))
                {
                    m_objectsInsideRoom.Remove(watchedObject);
                    if (m_objectsInsideRoom.Contains(watchedObject))
                    {
                        m_objectsOnEnterRoom.Remove(watchedObject);
                    }
                    m_objectsOnLeaveRoom.Add(watchedObject);
                }
                else if (m_objectsOnLeaveRoom.Contains(watchedObject))
                {
                    m_objectsOnLeaveRoom.Remove(watchedObject);
                }
            }
        }
    }
    #endregion

    public bool ObjectEntersRoom(Transform t)
    {
        if(!m_watchedObjects.Contains(t))
        {
            m_watchedObjects.Add(t);
            if(ObjectIsInsideRoom(t))
            {
                m_objectsInsideRoom.Add(t);
            }
        }

        return m_objectsOnEnterRoom.Contains(t);
    }

    public bool ObjectLeavesRoom(Transform t)
    {
        if (!m_watchedObjects.Contains(t))
        {
            m_watchedObjects.Add(t);
            if (ObjectIsInsideRoom(t))
            {
                m_objectsInsideRoom.Add(t);
            }
        }

        return m_objectsOnLeaveRoom.Contains(t);
    }

    public bool ObjectIsInsideRoom(Transform t)
    {
        return GetBounds().Contains(t.position);
    }

    public bool ObjectIsOutsideRoom(Transform t)
    {
        return !ObjectIsInsideRoom(t);
    }

    public void OnDrawGizmos()
    {
        //Bounds roomBounds = GetBounds();
        //Gizmos.DrawWireCube(roomBounds.center, roomBounds.extents * 2);
    }

    #region Accessors
    public Vector3 size
    {
        get { return m_size; }
        set { m_size = value; }
    }

    public RoomSkin skin
    {
        get { return m_skin; }
        set { m_skin = value; }
    }

    public Bounds GetBounds()
    {
        return new Bounds(transform.position + Vector3.up * m_size.y / 2, m_size);
    }
    #endregion

    #region Equals/GetHashCode
    public override bool Equals(object other)
    {
        return ReferenceEquals(other, this);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    #endregion

    #region Direction Helpers
    static public Direction ReciprocalDirection(Direction direction)
    {
        switch(direction)
        {
            case Direction.Left:
                return Direction.Right;
            case Direction.Forward:
                return Direction.Backward;
            case Direction.Right:
                return Direction.Left;
            case Direction.Backward:
                return Direction.Forward;
        }

        return Direction.Right;
    }

    static public Vector3 DirectionToVector(Direction direction)
    {
        switch (direction)
        {
            case Direction.Right:
                return Vector3.right;
            case Direction.Forward:
                return Vector3.forward;
            case Direction.Left:
                return Vector3.left;
            case Direction.Backward:
                return Vector3.back;
        }

        return Vector3.zero;
    }

    static public Vector3 DirectionScale(Direction direction, Vector3 size)
    {
        switch (direction)
        {
            case Direction.Right:
            case Direction.Left:
                return Vector3.Scale(Vector3.forward, size);
            case Direction.Backward:
            case Direction.Forward:
                return Vector3.Scale(Vector3.right, size);
        }

        return Vector3.zero;
    }

    static public float DirectionAngle(Direction direction)
    {
        switch (direction)
        {
            case Direction.Right:
                return 0;
            case Direction.Forward:
                return 270;
            case Direction.Left:
                return 180;
            case Direction.Backward:
                return 90;
        }

        return 0;
    }

    static public Direction[] allDirection
    {
        get { return s_allDirections; }
    }
    #endregion
}

[Serializable]
public class WallsDictionary : SeriDictionary<Room.Direction, RoomParts.WallPart>
{

}