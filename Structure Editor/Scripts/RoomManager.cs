using UnityEngine;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RoomManager : MonoBehaviour
{
    static private RoomManager s_singleton;
    // Prefabs
    [SerializeField] private Room[] m_roomPrefabs;
    [SerializeField] private RoomSkin[] m_roomSkins;
    [SerializeField] private RoomGraph m_roomsGraph;

    [SerializeField] private Room m_tmpRoom;
    [SerializeField] private int m_tmpRoomModelIndex = 0;

    [SerializeField] private int m_tmpRoomAnchorIndex = 0;
    [SerializeField] private Room.Direction m_tmpRoomRelation;

    private void Awake ()
    {
        s_singleton = this;
    }

#if UNITY_EDITOR
    private void UpdateTmpRoomModel()
    {
        if((m_roomPrefabs == null) || (m_roomPrefabs.Length == 0))
        {
            _UpdateRoomPrefabsList();
        }

        if(m_tmpRoom != null)
        {
            DestroyImmediate(m_tmpRoom.gameObject);
        }

        m_tmpRoom = Instantiate(m_roomPrefabs[m_tmpRoomModelIndex]);
        m_tmpRoom.transform.SetParent(transform);
    }

    public void CreateRoom()
    {
        Room[] existingRooms = GetExistingRooms();
        m_roomsGraph.AddNode(m_tmpRoom);
        if ((existingRooms != null) && (existingRooms.Length > 0))
        {
            Room anchorRoom = existingRooms[m_tmpRoomAnchorIndex];
            m_roomsGraph.AddLink(anchorRoom, m_tmpRoom, m_tmpRoomRelation, Room.ReciprocalDirection(m_tmpRoomRelation), UniGraph<Room, Room.Direction>.LinkType.OneWay);
        }
        m_tmpRoom.Configure();

        m_tmpRoom = null;
        UpdateTmpRoomModel();
    }
#endif

    #region Getters
    public string[] GetExistingRoomsNames()
    {
        return GetRoomsNames(m_roomsGraph.GetNodesValues());
    }

#if UNITY_EDITOR
    public string[] GetPrefabRoomsNames()
    {
        return GetRoomsNames(GetRoomPrefabs());
    }
#endif

    public Room[] GetExistingRooms()
    {
        return m_roomsGraph.GetNodesValues();
    }

    public bool GetHasExistingRooms()
    {
        Room[] existingRooms = GetExistingRooms();
        return (existingRooms != null) && (existingRooms.Length > 0);
    }

    static public RoomGraph roomsGraph
    {
        get { return singleton.m_roomsGraph; }
    }

#if UNITY_EDITOR
    public Room tmpRoom
    {
        get
        {
            if(m_tmpRoom == null)
            {
                UpdateTmpRoomModel();
            }
            return m_tmpRoom;
        }
    }

    public int tmpRoomModelIndex
    {
        get { return m_tmpRoomModelIndex; }
        set
        {
            if(value != m_tmpRoomModelIndex)
            {
                m_tmpRoomModelIndex = value;
                UpdateTmpRoomModel();
            }
            else
            {
                m_tmpRoomModelIndex = value;
            }
        }
    }
#endif

    public int tmpRoomAnchorIndex
    {
        get { return m_tmpRoomAnchorIndex; }
        set { m_tmpRoomAnchorIndex = value; }
    }

    public Room.Direction tmpRoomRelation
    {
        get { return m_tmpRoomRelation; }
        set { m_tmpRoomRelation = value; }
    }

    static public string[] GetRoomsNames(Room[] rooms)
    {
        if (rooms == null) return null;
        string[] result = new string[rooms.Length];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = rooms[i].name;
        }

        return result;
    }

    private Dictionary<Room.Direction, Room> _GetLinkedRooms(Room room)
    {
        Room[] linkedRooms = m_roomsGraph.GetLinkedNodesValues(room, UniGraph<Room, Room.Direction>.LinkType.Reverse);
        Room.Direction[] linkedRoomsDirections = m_roomsGraph.GetLinksValues(room, UniGraph<Room, Room.Direction>.LinkType.Reverse);

        Dictionary<Room.Direction, Room> linkedRoomsRelationsDictionary = new Dictionary<Room.Direction, Room>();
        for(int i = 0; i < linkedRooms.Length; i++)
        {
            linkedRoomsRelationsDictionary.Add(linkedRoomsDirections[i], linkedRooms[i]);
        }

        return linkedRoomsRelationsDictionary;
    }

    static public Dictionary<Room.Direction, Room> GetLinkedRooms(Room room)
    {
        return singleton._GetLinkedRooms(room);
    }
#endregion

    static public RoomManager singleton
    {
        get
        {
            if (s_singleton == null)
            {
                s_singleton = FindObjectOfType<RoomManager>();
                if (s_singleton == null)
                {
                    s_singleton = new GameObject("Room manager").AddComponent<RoomManager>();
                }

#if UNITY_EDITOR
                s_singleton._UpdateRoomPrefabsList();
                s_singleton._UpdateRoomSkinsList();
                if(s_singleton.m_roomsGraph == null)
                {
                    s_singleton.m_roomsGraph = new RoomGraph();
                }
#endif
            }

            return s_singleton;
        }
    }

#region Data updates
    [ContextMenu("Refresh existing rooms")]
    public void UpdateExistingRooms()
    {
        Room[] existingRooms = FindObjectsOfType<Room>();
        for(int i = 0; i < existingRooms.Length; i++)
        {
            if (existingRooms[i] == m_tmpRoom) continue;
            if (!m_roomsGraph.Contains(existingRooms[i]))
            {
                m_roomsGraph.AddNode(existingRooms[i]);
            }
        }

        List<Room> tmpExistingRooms = new List<Room>(existingRooms);
        Room[] roomNodes = m_roomsGraph.GetNodesValues();
        for(int i = 0; i < roomNodes.Length; i++)
        {
            if(!tmpExistingRooms.Contains(roomNodes[i]))
            {
                m_roomsGraph.RemoveAll(roomNodes[i]);
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("UpdateRoomPrefabsList")]
    private void _UpdateRoomPrefabsList()
    {
        m_roomPrefabs = AssetHelpers.GetPrefabs<Room>();
    }

    private Room[] _GetRoomPrefabs()
    {
        if ((m_roomPrefabs == null) || (m_roomPrefabs.Length == 0)) _UpdateRoomPrefabsList();
        return m_roomPrefabs;
    }

    [ContextMenu("UpdateRoomSkinsList")]
    private void _UpdateRoomSkinsList()
    {
        m_roomSkins = AssetHelpers.GetPrefabs<RoomSkin>();
    }

    private RoomSkin[] _GetRoomSkins()
    {
        if ((m_roomSkins == null) || (m_roomSkins.Length == 0)) _UpdateRoomSkinsList();
        return m_roomSkins;
    }

    static public void UpdateRoomPrefabsList()
    {
        singleton._UpdateRoomPrefabsList();
    }

    static public Room[] GetRoomPrefabs(bool update = false)
    {
        if (update) singleton._UpdateRoomPrefabsList();
        return singleton._GetRoomPrefabs();
    }

    static public void UpdateRoomSkinsList()
    {
        singleton._UpdateRoomSkinsList();
    }

    static public RoomSkin[] GetRoomSkins(bool update = false)
    {
        if (update) singleton._UpdateRoomSkinsList();
        return singleton._GetRoomSkins();
    }

    static public Room RoomFromHash(string hash)
    {
        Room[] roomPrefabs = GetRoomPrefabs();

        for (int i = 0; i < roomPrefabs.Length; i++)
        {
            if (roomPrefabs[i].name == hash)
            {
                return roomPrefabs[i];
            }
        }

        Debug.LogError("No room found with hash \"" + hash + "\"");

        return null;
    }
#endif
#endregion
}

[Serializable]
public class RoomGraph : UniGraph<Room, Room.Direction>
{
    public void RemoveAllNull()
    {
        List<UniNode> nodesToRemove = new List<UniNode>();
        foreach (UniNode node in m_nodes)
        {
            if (node.value == null)
            {
                nodesToRemove.Add(node);
            }
        }

        for (int i = 0; i < nodesToRemove.Count; i++)
        {
            RemoveNode(nodesToRemove[i]);
        }
    }
}
