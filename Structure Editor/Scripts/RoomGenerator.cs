using UnityEngine;
using System.Collections;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] private GameObject m_botWall;
    [SerializeField] private GameObject m_topWall;
    [SerializeField] private GameObject m_floor;
    [SerializeField] private GameObject m_ceil;
    [SerializeField] private GameObject m_light;
    [SerializeField] private GameObject m_door;

    [SerializeField] private Vector2 m_doorSize;

	private void Awake ()
    {
        
    }

    public void GenerateRooms(Room[] rooms)
    {
        float x = 0;
        for (int i = 0; i < rooms.Length; i++)
        {
            Vector3 roomSize = rooms[i].size;
            x += roomSize.x / 2;

            GameObject room = GenerateRoom(roomSize, (i == 0), (i == (rooms.Length - 1)));
            room.transform.position = new Vector3(x, 0, 0);
            room.name = string.Format("Room {0}", i);
            room.isStatic = true;

            GameObject light = Instantiate(m_light);
            light.transform.position = room.transform.position + 2.75f * Vector3.up;

            x += roomSize.x / 2;
        }
    }

    private GameObject GenerateSinglePart(Vector2 size, GameObject partBase)
    {
        GameObject part = Instantiate(partBase);

        MeshRenderer renderer = part.GetComponent<MeshRenderer>();
        Material material = renderer.material;
        size.Scale(material.mainTextureScale);
        material.mainTextureScale = size;
        //material.SetTextureScale("_MainTex", size);
        renderer.material = material;
        //part.isStatic = true;

        return part;
    }
	
    private GameObject GenerateWall(Vector2 size)
    {
        GameObject wall = new GameObject("Wall");

        Vector2 botSize = new Vector2(size.x, 1);
        size.y -= 1;

        GameObject botWall = GenerateSinglePart(botSize, m_botWall);
        botWall.transform.SetParent(wall.transform);
        botWall.transform.localScale = new Vector3(size.x, 1, 1);

        GameObject topWall = GenerateSinglePart(size, m_topWall);
        topWall.transform.SetParent(wall.transform);
        topWall.transform.localPosition = Vector3.up;
        topWall.transform.localScale = new Vector3(size.x, 1, size.y);

        return wall;
    }

    private GameObject GenerateWallWithDoor(Vector2 wallSize, float doorPosition, Vector2 doorSize, bool addDoor)
    {
        GameObject wallWithDoor = new GameObject("Wall with door");

        Vector2 botWallSize = new Vector2((wallSize.x - doorSize.x) / 2, 1);
        Vector2 topWallSize = new Vector2(wallSize.x, wallSize.y - doorSize.y);
        Vector2 midWallSize = new Vector2((wallSize.x - doorSize.x) / 2, doorSize.y - 1);

        GameObject botLeftWall = GenerateSinglePart(botWallSize, m_botWall);
        GameObject botRightWall = GenerateSinglePart(botWallSize, m_botWall);
        GameObject midLeftWall = GenerateSinglePart(midWallSize, m_topWall);
        GameObject midRightWall = GenerateSinglePart(midWallSize, m_topWall);
        GameObject topWall = GenerateSinglePart(topWallSize, m_topWall);

        botLeftWall.name = "Bottom left Wall";
        botRightWall.name = "Bottom right Wall";
        midLeftWall.name = "Mid left Wall";
        midRightWall.name = "Mid right Wall";
        topWall.name = "Top Wall";

        botLeftWall.transform.SetParent(wallWithDoor.transform);
        botRightWall.transform.SetParent(wallWithDoor.transform);
        midLeftWall.transform.SetParent(wallWithDoor.transform);
        midRightWall.transform.SetParent(wallWithDoor.transform);
        topWall.transform.SetParent(wallWithDoor.transform);
        

        botLeftWall.transform.localScale = new Vector3(botWallSize.x, 1, botWallSize.y);
        botRightWall.transform.localScale = new Vector3(botWallSize.x, 1, botWallSize.y);
        midLeftWall.transform.localScale = new Vector3(midWallSize.x, 1, midWallSize.y);
        midRightWall.transform.localScale = new Vector3(midWallSize.x, 1, midWallSize.y);
        topWall.transform.localScale = new Vector3(topWallSize.x, 1, topWallSize.y);

        botLeftWall.transform.localPosition = new Vector3(- (wallSize.x + doorSize.x) / 4, 0, 0);
        botRightWall.transform.localPosition = new Vector3((wallSize.x + doorSize.x) / 4, 0, 0);
        midLeftWall.transform.localPosition = new Vector3(-(wallSize.x + doorSize.x) / 4, 1, 0);
        midRightWall.transform.localPosition = new Vector3((wallSize.x + doorSize.x) / 4, 1, 0);
        topWall.transform.localPosition = new Vector3(0, doorSize.y, 0);
        
        if(addDoor)
        {
            GameObject door = Instantiate(m_door);
            door.transform.SetParent(wallWithDoor.transform);
            door.transform.localPosition = Vector3.zero;
        }

        return wallWithDoor;
    }

    private GameObject GenerateFloor(Vector2 size)
    {
        GameObject floor = GenerateSinglePart(size, m_floor);
        floor.transform.localScale = new Vector3(size.x, size.y, 1);
        floor.transform.Rotate(Vector3.right, 90, Space.World);

        return floor;
    }

    private GameObject GenerateCeil(Vector2 size)
    {
        GameObject ceil = GenerateSinglePart(size, m_ceil);
        ceil.transform.localScale = new Vector3(size.x, size.y, 1);
        ceil.transform.Rotate(Vector3.right, 270, Space.World);

        return ceil;
    }

    private GameObject GenerateRoom(Vector3 size, bool first, bool last)
    {
        GameObject room = new GameObject();

        for(int i = 0; i < 4; i++)
        {
            GameObject wall;
            float width = ((i % 2) == 0) ? size.x : size.z;
            Vector2 wallSize = new Vector2(width, size.y);

            if(((i == 3) && !first) || ((i == 1) && !last))
            {
                wall = GenerateWallWithDoor(wallSize, 0, m_doorSize, i == 1);
            }
            else
            {
                wall = GenerateWall(wallSize);
            }

            wall.name = string.Format("Wall {0}", i);
            wall.transform.SetParent(room.transform);
            float x = Mathf.Sin(Mathf.PI / 2 * i) * size.x / 2;
            float z = Mathf.Cos(Mathf.PI / 2 * i) * size.z / 2;
            wall.transform.localPosition = new Vector3(x, 0, z);
            wall.transform.Rotate(Vector3.up, i * 90 + 180, Space.World);
        }

        Vector2 floorCeilSize = new Vector2(size.x, size.z);

        GameObject floor = GenerateFloor(floorCeilSize);
        GameObject ceil = GenerateCeil(floorCeilSize);

        floor.transform.SetParent(room.transform);

        ceil.transform.SetParent(room.transform);
        ceil.transform.localPosition = new Vector3(0, size.y, 0);

        return room;
    }
}
