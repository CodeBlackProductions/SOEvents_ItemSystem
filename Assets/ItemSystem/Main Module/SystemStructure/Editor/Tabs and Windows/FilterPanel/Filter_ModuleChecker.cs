using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ItemSystem.Editor
{
    /// <summary>
    /// This class is used to check if an object contains a specific type or any of the types in a list.
    /// </summary>
    /// <remarks>
    /// The ContainsTypeRecursive method checks if the object or any of its properties contain the specified type.
    /// It uses reflection to get the properties of the object and checks if they are assignable to the target type.
    /// </remarks>
    public static class Filter_ModuleChecker
    {
        private static Type[] m_DictionaryInterfaces =
      {
          typeof(IDictionary<,>),
          typeof(System.Collections.IDictionary),
          typeof(IReadOnlyDictionary<,>),
        };

        private static int m_SelectedType = -1;

        public static bool ContainsAllObjects(object obj, List<ScriptableObject> requiredObjects)
        {
            m_SelectedType = -1;
            return requiredObjects.All(SO => ContainsObjectRecursive(obj, SO, new HashSet<object>()));
        }

        public static bool ContainsAnyObjects(object obj, List<ScriptableObject> requiredObjects)
        {
            m_SelectedType = -1;
            return requiredObjects.Any(SO => ContainsObjectRecursive(obj, SO, new HashSet<object>()));
        }

        private static bool ContainsObjectRecursive(object obj, ScriptableObject targetObject, HashSet<object> visited)
        {
            if (obj == null || visited.Contains(obj)) return false;

            visited.Add(obj);

            if (targetObject == obj as ScriptableObject) return true;

            PropertyInfo selectedTypeProp = obj.GetType().GetProperty("TypeIndex");
            if (selectedTypeProp != null)
            {
                m_SelectedType = (int)selectedTypeProp.GetValue(obj);
            }

            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (!property.CanRead) continue;

                if (typeof(ScriptableObject).IsAssignableFrom(property.PropertyType))
                {
                    var subItem = property.GetValue(obj) as ScriptableObject;
                    if (subItem != null)
                    {
                        if (targetObject == subItem) return true;

                        if (ContainsObjectRecursive(subItem, targetObject, visited)) return true;
                    }
                }
                else if (typeof(IEnumerable<ScriptableObject>).IsAssignableFrom(property.PropertyType))
                {
                    var subItems = property.GetValue(obj) as IEnumerable<ScriptableObject>;

                    if (m_SelectedType != -1 && property.Name == "Types")
                    {
                        var onlyType = subItems.ElementAt(m_SelectedType);
                        subItems = new List<ScriptableObject> { onlyType };
                    }

                    if (subItems != null)
                    {
                        foreach (var subItem in subItems)
                        {
                            if (subItem != null)
                            {
                                if (targetObject == subItem) return true;

                                if (ContainsObjectRecursive(subItem, targetObject, visited)) return true;
                            }
                        }
                    }
                }
                else if (IsDictionary(property.PropertyType))
                {
                    var subItems = property.GetValue(obj) as IEnumerable;

                    if (subItems != null)
                    {
                        foreach (var item in subItems)
                        {
                            var itemType = item.GetType();
                            var valueProperty = itemType.GetProperty("Value");

                            ScriptableObject value = valueProperty?.GetValue(item) as ScriptableObject;

                            if (value != null)
                            {
                                if (targetObject == value) return true;

                                if (ContainsObjectRecursive(value, targetObject, visited)) return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static bool IsDictionary(Type _Type)
        {
            return m_DictionaryInterfaces
             .Any(dictInterface =>
                 dictInterface == _Type ||
                 (_Type.IsGenericType && dictInterface == _Type.GetGenericTypeDefinition()) ||
                 _Type.GetInterfaces().Any(typeInterface =>
                                          typeInterface == dictInterface ||
                                          (typeInterface.IsGenericType && dictInterface == typeInterface.GetGenericTypeDefinition())));
        }
    }
}