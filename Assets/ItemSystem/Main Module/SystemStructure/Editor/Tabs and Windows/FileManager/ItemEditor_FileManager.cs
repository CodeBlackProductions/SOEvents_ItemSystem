using System.IO;
using UnityEditor;
using UnityEngine;

public static class ItemEditor_FileManager
{
    public static void SaveModuleToFile(ScriptableObject _Module)
    {
        SO_EditorSettings settings = ItemEditor_AssetLoader.LoadAssetByName<SO_EditorSettings>("EditorSettings");
        string directoryPath = settings.FilePath;

        string jsonString = JsonUtility.ToJson(_Module);
        
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        System.IO.File.WriteAllText($"{directoryPath}/{_Module.GetType().Name}_{(_Module as IItemModule).ModuleGUID}.json", jsonString);
    }

    public static void LoadModuleFromFile(ScriptableObject _Module)
    {
        ItemEditor_InstanceManager.CreateInstance(_Module, "ImportedModules");
    }
}