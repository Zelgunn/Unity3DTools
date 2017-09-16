using UnityEngine;
using System.Collections;
using System.Xml;
using System;

public class RoomPit : Room
{
    [SerializeField] protected Vector3 m_pitSize;
    //[SerializeField] protected Vector2 m_pitPosition;
    [SerializeField, HideInInspector] protected Transform m_floorStructure;

    protected override void ConfigureFloor()
    {
        m_floorStructure = new GameObject("Floor and Pit").transform;
        m_floorStructure.SetParent(m_roomStructure);
        m_floorStructure.localPosition = Vector3.zero;

        ConfigureFloorParts();
        ConfigurePit();
    }

    protected virtual void ConfigureFloorParts()
    {
        Vector2 floorSize = new Vector2(m_size.x, m_size.z);
        Vector2 startEndFloorSize = new Vector2((floorSize.x - m_pitSize.x) / 2, floorSize.y);
        Vector2 leftRightFloorSize = new Vector2(m_pitSize.x, (floorSize.y - m_pitSize.z) / 2);

        RoomParts.RoomPart startFloor = ConfigureFloorPart("Start", startEndFloorSize);
        RoomParts.RoomPart endFloor = ConfigureFloorPart("End", startEndFloorSize);
        RoomParts.RoomPart leftFloor = ConfigureFloorPart("Left", leftRightFloorSize);
        RoomParts.RoomPart rightFloor = ConfigureFloorPart("Right", leftRightFloorSize);

        float xPosition = (startEndFloorSize.x + m_pitSize.x) / 2;
        startFloor.transform.localPosition = Vector3.left * xPosition;
        endFloor.transform.localPosition = Vector3.right * xPosition;

        float yPosition = (leftRightFloorSize.y + m_pitSize.z) / 2;
        leftFloor.transform.localPosition = Vector3.forward * yPosition;
        rightFloor.transform.localPosition = Vector3.back * yPosition;
    }

    protected virtual RoomParts.RoomPart ConfigureFloorPart(string partName, Vector2 partSize)
    {
        RoomParts.RoomPart floorPart = Instantiate(m_skin.floor);
        floorPart.Configure(m_floorStructure, partName + " floor", partSize);

        return floorPart;
    }

    protected virtual void ConfigurePit()
    {
        RoomParts.PitPart pit = Instantiate(m_skin.pit);
        pit.Configure(m_floorStructure, "Pit", m_pitSize);
        pit.transform.localPosition = Vector3.zero;
    }
}