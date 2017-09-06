using UnityEngine;
using System.Collections;

namespace TriggerEditor
{
    static public class TE_GameObject
    {
        [NodeMethod("Objects", "Object is active")]
        static public bool GameObjectIsActive(GameObject gameObject)
        {
            return gameObject.activeInHierarchy;
        }

        [NodeMethod("Objects", "Set active")]
        static public void ActivateGameObject(GameObject gameobject, bool active)
        {
            gameobject.SetActive(active);
        }
    }
}