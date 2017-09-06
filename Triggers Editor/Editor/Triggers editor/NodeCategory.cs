using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace TriggerEditor
{
    [CreateAssetMenu(menuName = "Node category")]
    public class NodeCategory : ScriptableObject
    {
        static private Dictionary<string, NodeCategory> s_categoriesData;

        [SerializeField] private Texture2D m_icon;

        public Texture2D icon
        {
            get { return m_icon; }
        }

        static public NodeCategory[] FindAllCategories()
        {
            string[] categoriesAssetPaths = AssetDatabase.FindAssets("t:NodeCategory");
            NodeCategory[] result = new NodeCategory[categoriesAssetPaths.Length];
            for(int i = 0; i < categoriesAssetPaths.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(categoriesAssetPaths[i]);
                result[i] = AssetDatabase.LoadAssetAtPath(assetPath, typeof(NodeCategory)) as NodeCategory;
            }

            s_categoriesData = new Dictionary<string, NodeCategory>();
            foreach (NodeCategory category in result)
            {
                if (category != null) s_categoriesData.Add(category.name, category);
            }

            return result;
        }

        static public NodeCategory GetNodeCategory(string categoryName)
        {
            if (s_categoriesData == null) FindAllCategories();
            categoryName = CategoryFileName(categoryName);
            if (s_categoriesData.ContainsKey(categoryName)) return s_categoriesData[categoryName];
            return null;
        }

        static public string CategoryFileName(string categoryName)
        {
            if (categoryName.Contains("/"))
            {
                string[] tmp = categoryName.Split('/');
                categoryName = tmp[tmp.Length - 1];
            }
            categoryName = categoryName.Replace("|", string.Empty);

            return categoryName;
        }
    }
}