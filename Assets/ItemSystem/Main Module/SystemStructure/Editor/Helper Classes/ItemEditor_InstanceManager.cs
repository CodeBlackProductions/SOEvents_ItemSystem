using ItemSystem.MainModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Handles the creation and removal of instances of ScriptableObjects that implement the IItemModule interface.
    /// </summary>
    public static class ItemEditor_InstanceManager
    {
        public static void CreateInstance<T>(T _TemporarySOInstance, System.Type _ModuleType) where T : ScriptableObject
        {
            if (_TemporarySOInstance.IsConvertibleTo<IItemModule>(true))
            {
                string fileName = _TemporarySOInstance.GetType().Name;

                GUID instanceGUID;
                GUID nullGUID;
                GUID.TryParse("00000000000000000000000000000000", out nullGUID);
                if ((_TemporarySOInstance as IItemModule).ModuleGUID == null || (_TemporarySOInstance as IItemModule).ModuleGUID == nullGUID)
                {
                    instanceGUID = GUID.Generate();
                    (_TemporarySOInstance as IItemModule).ModuleGUID = instanceGUID;
                }
                else
                {
                    instanceGUID = (_TemporarySOInstance as IItemModule).ModuleGUID;
                }
                fileName += $"_{instanceGUID}";

                SO_EditorSettings settings = ItemEditor_AssetLoader.LoadAssetByName<SO_EditorSettings>("EditorSettings");

                string assetPath = $"{settings.InstancesPath}/{_ModuleType.Name}";
                string path = $"{assetPath}/{fileName}.asset";
                if (!string.IsNullOrEmpty(path))
                {
                    if (!Directory.Exists(assetPath))
                    {
                        Directory.CreateDirectory(assetPath);
                    }
                    if (File.Exists(path))
                    {
                        Debug.LogWarning($"Asset with the name {fileName} at {assetPath} already exists.");
                        return;
                    }
                    AssetDatabase.CreateAsset(_TemporarySOInstance, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

        public static void CopyInstance<T>(T _CopySOInstance, System.Type _ModuleType)
        {
            ScriptableObject temporarySOInstance = ScriptableObject.CreateInstance(_ModuleType);

            foreach (var property in _ModuleType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.CanRead && property.CanWrite)
                {
                    var value = property.GetValue(_CopySOInstance);
                    if (property.Name == "ModuleName")
                    {
                        value += "_Copy";
                    }
                    if (property.Name == "ModuleGUID")
                    {
                        value = null;
                    }
                    property.SetValue(temporarySOInstance, DeepCopyValue(value));
                }
            }

            CreateInstance(temporarySOInstance, _ModuleType);
        }

        public static void RemoveInstance<T>(T _Instance) where T : ScriptableObject
        {
            if (_Instance.IsConvertibleTo<IItemModule>(true))
            {
                string fileName = _Instance.GetType().Name;
                GUID instanceGUID = (_Instance as IItemModule).ModuleGUID;
                fileName += $"_{instanceGUID}";
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

                if (asset == null)
                {
                    return;
                }

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

        private static object DeepCopyValue(object _Value)
        {
            if (_Value == null)
                return null;

            var type = _Value.GetType();

            if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(GUID))
                return _Value;

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                return _Value;

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var array = _Value as Array;
                var copied = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++)
                    copied.SetValue(DeepCopyValue(array.GetValue(i)), i);
                return copied;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var listType = typeof(List<>).MakeGenericType(type.GetGenericArguments());
                var list = (System.Collections.IList)Activator.CreateInstance(listType);
                foreach (var item in (System.Collections.IEnumerable)_Value)
                    list.Add(DeepCopyValue(item));
                return list;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var args = type.GetGenericArguments();
                var dictType = typeof(Dictionary<,>).MakeGenericType(args);
                var dict = (System.Collections.IDictionary)Activator.CreateInstance(dictType);
                var original = (System.Collections.IDictionary)_Value;
                foreach (var key in original.Keys)
                    dict.Add(DeepCopyValue(key), DeepCopyValue(original[key]));
                return dict;
            }

            var clone = Activator.CreateInstance(type);
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    var propValue = prop.GetValue(_Value);
                    prop.SetValue(clone, DeepCopyValue(propValue));
                }
            }
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (!field.IsInitOnly)
                {
                    var fieldValue = field.GetValue(_Value);
                    field.SetValue(clone, DeepCopyValue(fieldValue));
                }
            }
            return clone;
        }
    }
}