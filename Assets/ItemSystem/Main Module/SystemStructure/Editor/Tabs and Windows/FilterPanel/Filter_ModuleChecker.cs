using ItemSystem.MainModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

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
        public static bool ContainsAllObjects(object obj, List<ScriptableObject> requiredObjects)
        {
            return requiredObjects.All(SO => ContainsObjectRecursive(obj, SO, new HashSet<object>()));
        }

        public static bool ContainsAnyObjects(object obj, List<ScriptableObject> requiredObjects)
        {
            return requiredObjects.Any(SO => ContainsObjectRecursive(obj, SO, new HashSet<object>()));
        }

        private static bool ContainsObjectRecursive(object obj, ScriptableObject targetObject, HashSet<object> visited)
        {
            if (obj == null || visited.Contains(obj)) return false;

            visited.Add(obj);

            if (targetObject == obj as ScriptableObject) return true;

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
            }
            return false;
        }
    }
}