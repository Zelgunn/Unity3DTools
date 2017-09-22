using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TriggerEditor
{
    public class TE_Area : MonoBehaviour
    {
        Dictionary<GameObject, int> m_containedObjects = new Dictionary<GameObject, int>();
        List<GameObject> m_exitingObjects = new List<GameObject>();

        private void OnTriggerEnter(Collider other)
        {
            if(!m_containedObjects.ContainsKey(other.gameObject))
            {
                m_containedObjects.Add(other.gameObject, Time.frameCount);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (m_containedObjects.ContainsKey(other.gameObject))
            {
                m_containedObjects.Remove(other.gameObject);
                m_exitingObjects.Add(other.gameObject);
            }
        }

        private void LateUpdate()
        {
            m_exitingObjects.Clear();
        }

        [NodeMethod("Areas", "Contains gameobject", NodeMethodType.Condition)]
        public bool ContainsObject(GameObject gameObject)
        {
            return m_containedObjects.ContainsKey(gameObject);
        }

        [NodeMethod("Areas", "Object enters", NodeMethodType.Event)]
        public bool ObjectEnters(GameObject gameObject)
        {
            if(m_containedObjects.ContainsKey(gameObject))
            {
                return m_containedObjects[gameObject] == Time.frameCount;
            }
            else
            {
                return false;
            }
        }

        [NodeMethod("Areas", "Object exits", NodeMethodType.Event)]
        public bool ObjectExits(GameObject gameObject)
        {
            return m_exitingObjects.Contains(gameObject);
        }
    }
}