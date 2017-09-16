using UnityEngine;
using System.Collections;

namespace RoomParts
{
    public class WallDoorPart : WallPart
    {
        static protected readonly Vector2 s_defaultDoorSize = new Vector2(1.4f, 2.55f);
        [SerializeField] protected DoorPart m_doorPart;

        public virtual void Configure(Transform parent, string partName, Vector2 size, float bottomHeight, Vector2 doorSize, bool addDoor, bool makeStatic = true)
        {
            Setup(parent, partName, size);

            Vector2 bottomSize = new Vector2((size.x - doorSize.x) / 2, s_defaultBottomHeight);
            Vector2 midSize = new Vector2((size.x - doorSize.x) / 2, doorSize.y - 1);
            Vector2 topSize = new Vector2(size.x, size.y - doorSize.y);

            RoomPart leftBottomWall = Instantiate(m_bottomWall);
            RoomPart rightBottomWall = Instantiate(m_bottomWall);
            RoomPart leftMidWall = Instantiate(m_topWall);
            RoomPart rightMidWall = Instantiate(m_topWall);
            RoomPart topWall = Instantiate(m_topWall);

            leftBottomWall.Configure(transform, name + "_lb", bottomSize, makeStatic);
            rightBottomWall.Configure(transform, name + "_rb", bottomSize, makeStatic);
            leftMidWall.Configure(transform, name + "_lm", midSize, makeStatic);
            rightMidWall.Configure(transform, name + "_rm", midSize, makeStatic);
            topWall.Configure(transform, name + "_top", topSize, makeStatic);

            leftBottomWall.transform.localPosition = new Vector3((m_size.x + doorSize.x) / 4, 0, 0);
            rightBottomWall.transform.localPosition = new Vector3(-(m_size.x + doorSize.x) / 4, 0, 0);
            leftMidWall.transform.localPosition = new Vector3((m_size.x + doorSize.x) / 4, bottomHeight, 0);
            rightMidWall.transform.localPosition = new Vector3(-(m_size.x + doorSize.x) / 4, bottomHeight, 0);
            topWall.transform.localPosition = new Vector3(0, doorSize.y, 0);

            if(addDoor)
            {
                DoorPart door = Instantiate(m_doorPart);
                door.Configure(transform, "Door", doorSize, makeStatic);
                door.transform.localPosition = Vector3.zero;
            }

            gameObject.isStatic = makeStatic;
        }

        public virtual void Configure(Transform parent, string partName, Vector2 size, Vector2 doorSize, bool addDoor, bool makeStatic = true)
        {
            Configure(parent, partName, size, s_defaultBottomHeight, doorSize, addDoor, makeStatic);
        }

        public override void Configure(Transform parent, string partName, Vector2 size, float bottomHeight, bool makeStatic = true)
        {
            Configure(parent, partName, size, bottomHeight, s_defaultDoorSize, false, makeStatic);
        }

        public override void Configure(Transform parent, string partName, Vector2 size, bool makeStatic = true)
        {
            Configure(parent, partName, size, s_defaultBottomHeight, s_defaultDoorSize, false, makeStatic);
        }
    }
}