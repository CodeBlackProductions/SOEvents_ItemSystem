using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SO_FileManager : ScriptableObject, IItemModule
{
    [SerializeField] private ScriptableObject[] m_LoadedModules;
    [SerializeField] private ScriptableObject[] m_LocalFiles;

    [SerializeField] private GUID m_GUID;

    [ItemToolkitAccess] public ScriptableObject[] LoadedModules { get => m_LoadedModules = LoadModules(); set => m_LoadedModules = LoadModules(); }
    [ItemToolkitAccess] public ScriptableObject[] LocalFiles { get => m_LocalFiles = LoadLocalFiles(); set => m_LocalFiles = LoadLocalFiles(); }

    public string ModuleName { get => "FileManagerData"; set => Debug.LogWarning("System modules should not be changed."); }
    public GUID ModuleGUID { get => m_GUID; set => Debug.LogWarning("System modules should not be changed."); }

    private void OnEnable()
    {
        m_GUID = GUID.Generate();
    }

    private ScriptableObject[] LoadModules()
    {
        List<ScriptableObject> loadedModules = ItemEditor_AssetLoader.LoadAssetsByType<ScriptableObject>();
        List<ScriptableObject> sortedModules = new List<ScriptableObject>();

        foreach (var module in loadedModules)
        {
            if (typeof(IItemModule).IsAssignableFrom(module.GetType()) && (module as IItemModule).ModuleName != "EditorSettings" && (module as IItemModule).ModuleName != "FileManagerData")
            {
                sortedModules.Add(module);
            }
        }

        return sortedModules.ToArray();
    }

    private ScriptableObject[] LoadLocalFiles()
    {
        SO_EditorSettings settings = ItemEditor_AssetLoader.LoadAssetByName<SO_EditorSettings>("EditorSettings");

        List<ScriptableObject> localFiles = new List<ScriptableObject>();

        if (Directory.Exists(settings.FilePath))
        {
            string[] files = Directory.GetFiles(settings.FilePath, "*.json");
            foreach (var file in files)
            {
                string jsonContent = File.ReadAllText(file);

                string fileType = Path.GetFileNameWithoutExtension(file);
                fileType = fileType.Substring(0, fileType.LastIndexOf('_'));
                ScriptableObject so = ScriptableObject.CreateInstance(fileType);
                JsonUtility.FromJsonOverwrite(jsonContent, so);
                localFiles.Add(so);
            }
        }
        return localFiles.ToArray();
    }
}