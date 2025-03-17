using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ItemEditorAssetLoader
{
    public static T LoadAssetByName<T>(string _AssetName) where T : Object
    {
        string[] guids = AssetDatabase.FindAssets(_AssetName + " t:" + typeof(T).Name);
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        Debug.LogError($"Asset '{_AssetName}' of type {typeof(T).Name} not found.");
        return null;
    }

    public static List<T> LoadAssetsByType<T>() where T : Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

        return guids.Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid))).ToList();
    }
}