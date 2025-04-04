using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        AssetDatabase.Refresh();
    }

    public static List<ScriptableObject> LoadModulesFromFiles(Type _ModuleType)
    {
        SO_EditorSettings settings = ItemEditor_AssetLoader.LoadAssetByName<SO_EditorSettings>("EditorSettings");

        List<ScriptableObject> localFiles = new List<ScriptableObject>();

        if (Directory.Exists(settings.FilePath))
        {
            string[] files = Directory.GetFiles(settings.FilePath, "*.json");
            foreach (var file in files)
            {
                string jsonContent = File.ReadAllText(file);
                string fileTypeName = Path.GetFileNameWithoutExtension(file);
                fileTypeName = fileTypeName.Substring(0, fileTypeName.LastIndexOf('_'));
                Type fileType = GetTypeByName(fileTypeName);

                if (fileType != null && _ModuleType.IsAssignableFrom(fileType))
                {
                    ScriptableObject so = ScriptableObject.CreateInstance(fileTypeName);
                    JsonUtility.FromJsonOverwrite(jsonContent, so);
                    localFiles.Add(so);
                }
            }
        }
        return localFiles;
    }

    private static Type GetTypeByName(string _TypeName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .FirstOrDefault(type => type.Name == _TypeName);
    }
}