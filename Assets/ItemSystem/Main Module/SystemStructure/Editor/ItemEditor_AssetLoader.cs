using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class ItemEditor_AssetLoader
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

    public static IEnumerable<ScriptableObject> LoadAssetsByTypeReference(System.Type _Type)
    {
        MethodInfo method = typeof(ItemEditor_AssetLoader).GetMethod("LoadAssetsByType", BindingFlags.Public | BindingFlags.Static);
        MethodInfo generic = method.MakeGenericMethod(_Type);
        return (IEnumerable<ScriptableObject>)generic.Invoke(null, null);
    }
}