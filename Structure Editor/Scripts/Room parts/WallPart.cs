using UnityEngine;
using System.Collections;

namespace RoomParts
{
    public class WallPart : RoomPart
    {
        static protected readonly float s_defaultBottomHeight = 1;

        [SerializeField] protected RoomPart m_bottomWall;
        [SerializeField] protected RoomPart m_topWall;

        public virtual void Configure(Transform parent, string partName, Vector2 size, float bottomHeight, bool makeStatic = true)
        {
            Setup(parent, partName, size);

            RoomPart bottomWall = Instantiate(m_bottomWall);
            RoomPart topWall = Instantiate(m_topWall);

            Vector2 bottomSize = new Vector2(m_size.x, bottomHeight);
            Vector2 topSize = m_size + Vector2.down * bottomHeight;

            bottomWall.Configure(transform, name + "_bottom", bottomSize, makeStatic);
            topWall.Configure(transform, name + "_top", topSize, makeStatic);

            bottomWall.transform.localPosition = Vector3.zero;
            topWall.transform.localPosition = Vector3.up * bottomHeight;

            gameObject.isStatic = makeStatic;
        }

        public override void Configure(Transform parent, string partName, Vector2 size, bool makeStatic = true)
        {
            Configure(parent, partName, size, s_defaultBottomHeight, makeStatic);
        }
    }
}