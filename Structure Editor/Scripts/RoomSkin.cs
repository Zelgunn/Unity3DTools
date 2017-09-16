using UnityEngine;
using System.Collections;

public class RoomSkin : MonoBehaviour
{
    [SerializeField] protected RoomParts.WallPart m_wall;
    [SerializeField] protected RoomParts.WallDoorPart m_wallWithDoor;
    [SerializeField] protected RoomParts.RoomPart m_floor;
    [SerializeField] protected RoomParts.RoomPart m_ceiling;
    [SerializeField] protected RoomParts.DoorPart m_door;

    [SerializeField] protected Vector2 m_doorSize = new Vector2(1.4f, 2.55f);

    // Room pit
    [SerializeField] protected RoomParts.PitPart m_pit;

    #region Getters
    public RoomParts.WallPart wall
    {
        get { return m_wall; }
    }

    public RoomParts.WallDoorPart wallWithDoor
    {
        get { return m_wallWithDoor; }
    }

    public RoomParts.RoomPart floor
    {
        get { return m_floor; }
    }

    public RoomParts.RoomPart ceiling
    {
        get { return m_ceiling; }
    }

    public RoomParts.DoorPart door
    {
        get { return m_door; }
    }

    public Vector2 doorSize
    {
        get { return m_doorSize; }
    }

    public RoomParts.PitPart pit
    {
        get { return m_pit; }
    }
    #endregion
}
