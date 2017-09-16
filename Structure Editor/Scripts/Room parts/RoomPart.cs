using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RoomParts
{
    public class RoomPart : MonoBehaviour
    {
        protected Vector2 m_size;

        protected virtual void Setup(Transform parent, string partName, Vector2 size)
        {
            name = partName;
            m_size = size;
            transform.SetParent(parent);
        }

        public virtual void Configure(Transform parent, string partName, Vector2 size, bool makeStatic = true)
        {
            Setup(parent, partName, size);
            transform.localScale = new Vector3(size.x, 1, size.y);

            MeshRenderer renderer = GetComponent<MeshRenderer>();
            Material material = new Material(renderer.sharedMaterial);
            material.mainTextureScale = Vector3.Scale(size, material.mainTextureScale);
            renderer.material = material;

            gameObject.isStatic = makeStatic;

            GeneratePartMaterialAsset(material);
        }

        public Room GetParentRoom()
        {
            Transform parent = transform.parent;
            while (parent != null)
            {
                Room room = parent.GetComponent<Room>();
                if (room != null)
                {
                    return room;
                }

                parent = parent.parent;
            }
            return null;
        }

        public Vector2 size
        {
            get { return m_size; }
        }

        public void GeneratePartMaterialAsset(Material material)
        {
#if UNITY_EDITOR
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string roomName = GetParentRoom().name;
            string materialsDirectory = string.Format("Assets/tmp/Materials/{0}/{1}", sceneName, roomName);

            if (!Directory.Exists(materialsDirectory))
            {
                Directory.CreateDirectory(materialsDirectory);
            }

            string materialName = "";
            Transform parent = transform.parent;
            while(parent != null)
            {
                materialName += parent.name + ' ';

                Room room = parent.GetComponent<Room>();
                if (room != null)
                {
                    break;
                }

                parent = parent.parent;
            }

            materialName += name + "_material";
            string assetPath = string.Format("{0}/{1}_{2}.mat", materialsDirectory, roomName, materialName);
            AssetDatabase.CreateAsset(material, assetPath);
#endif
        }
    }

}
