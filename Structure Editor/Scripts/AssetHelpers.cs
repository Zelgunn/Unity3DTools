using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;

public class AssetHelpers
{
    static public T[] GetPrefabs<T>()
    {
        List<T> roomPrefabs = new List<T>();
        string[] paths = System.IO.Directory.GetFiles("Assets", "*.prefab", System.IO.SearchOption.AllDirectories);
        foreach (string path in paths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            T room = prefab.GetComponent<T>();
            if (room != null)
            {
                roomPrefabs.Add(room);
            }
        }
        return roomPrefabs.ToArray();
    }

    static public string[] GetSkinsNames(RoomSkin[] skins)
    {
        if (skins == null) return null;
        string[] result = new string[skins.Length];
        for (int i = 0; i < skins.Length; i++)
        {
            result[i] = skins[i].name;
        }
        return result;
    }
}
#endif