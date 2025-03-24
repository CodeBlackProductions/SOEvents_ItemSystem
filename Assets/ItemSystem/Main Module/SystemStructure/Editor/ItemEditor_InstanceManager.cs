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
            string fileName = _TemporarySOInstance.GetType().Name;
            GUID instanceGUID = GUID.Generate();
            (_TemporarySOInstance as IItemModule).ModuleGUID = instanceGUID;
            fileName += $"_{instanceGUID}";

            if (m_ModuleRegistry.ContainsKey(fileName))
            {
                Debug.LogError($"Instance with name '{fileName}' already exists.");
                return;
            }

            string assetPath = $"Assets/ItemSystem/SO_Instances/{_ModuleType}/";
            string path = $"{assetPath}{fileName}.asset";
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(_TemporarySOInstance, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (!m_ModuleRegistry.ContainsKey(fileName))
                {
                    m_ModuleRegistry.Add(fileName, instanceGUID);
                }
            }
        }
    }

    public static void RemoveInstance<T>(T _Instance) where T : ScriptableObject
    {
        if (_Instance.IsConvertibleTo<IItemModule>(true))
        {
            string fileName = _Instance.GetType().Name;
            GUID instanceGUID = (_Instance as IItemModule).ModuleGUID;
            fileName += $"_{instanceGUID}";
            if (m_ModuleRegistry.ContainsKey(fileName))
            {
                m_ModuleRegistry.Remove(fileName);
            }
            string path = AssetDatabase.GetAssetPath(_Instance);
           
            UpdateReferences(_Instance);
            
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }
    }

    private static void UpdateReferences(ScriptableObject _DeletedInstance)
    {
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

            if (asset != null)
            {
                SerializedObject serializedObject = new SerializedObject(asset);
                SerializedProperty property = serializedObject.GetIterator();

                while (property.NextVisible(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == _DeletedInstance)
                    {
                        property.objectReferenceValue = null;
                        serializedObject.ApplyModifiedProperties();
                    }
                    else if (property.isArray && property.propertyType == SerializedPropertyType.Generic)
                    {
                        for (int i = 0; i < property.arraySize; i++)
                        {
                            SerializedProperty element = property.GetArrayElementAtIndex(i);
                            if (element.propertyType == SerializedPropertyType.ObjectReference && element.objectReferenceValue == _DeletedInstance)
                            {
                                property.DeleteArrayElementAtIndex(i);
                                serializedObject.ApplyModifiedProperties();
                                i--;
                            }
                        }
                    }
                }
            }
        }
    }
}