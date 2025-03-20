using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public static class ItemEditor_InstanceManager
{
    private static Dictionary<string, GUID> m_ModuleRegistry = new Dictionary<string, GUID>();

    public static void CreateInstance<T>(T _TemporarySOInstance, string _ModuleType) where T : ScriptableObject
    {
        if (_TemporarySOInstance.IsConvertibleTo<IItemModule>(true))
        {
            string instanceName = (_TemporarySOInstance as IItemModule).ModuleName;
            GUID instanceGUID = (_TemporarySOInstance as IItemModule).ModuleGUID = GUID.Generate();
            instanceName += $"_{instanceGUID}";

            if (m_ModuleRegistry.ContainsKey(instanceName))
            {
                Debug.LogError($"Instance with name '{instanceName}' already exists.");
                return;
            }

            string assetPath = $"Assets/ItemSystem/SO_Instances/{_ModuleType}/";
            string path = $"{assetPath}{instanceName}.asset";
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(_TemporarySOInstance, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (!m_ModuleRegistry.ContainsKey(instanceName))
                {
                    m_ModuleRegistry.Add(instanceName, instanceGUID);
                }
            }
        }
    }
}