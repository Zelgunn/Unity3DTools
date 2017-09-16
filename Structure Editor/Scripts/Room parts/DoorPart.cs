using UnityEngine;
using System.Collections;

namespace RoomParts
{
    public class DoorPart : RoomPart
    {
        public override void Configure(Transform parent, string partName, Vector2 size, bool makeStatic = true)
        {
            Setup(parent, partName, size);
        }
    }
}