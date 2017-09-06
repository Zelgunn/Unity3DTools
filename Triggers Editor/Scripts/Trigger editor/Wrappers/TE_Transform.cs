using UnityEngine;
using System.Collections;

namespace TriggerEditor
{
    static public class TE_Transform
    {
        [NodeMethod("Objects", "Move by")]
        static public void MoveBy(Transform transform, Vector3 delta)
        {
            transform.position += delta;
        }

        [NodeMethod("Objects", "Move to")]
        static public void MoveTo(Transform transform, Vector3 position)
        {
            transform.position = position;
        }

        [NodeMethod("Objects", "Position of")]
        static public Vector3 GetPosition(Transform transform)
        {
            return transform.position;
        }

        [NodeMethod("Objects", "A is close to B")]
        static public bool AIsCloseToB(Transform a, Transform b, float maxDistance)
        {
            return (a.position - b.position).magnitude <= maxDistance;
        }
    }
}
