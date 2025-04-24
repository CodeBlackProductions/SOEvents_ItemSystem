using System;
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

            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead) continue;
                try
                {
                    var value = prop.GetValue(obj); if (value == null) continue;
                    if (targetObject == value as ScriptableObject) return true;
                    if (!(value is string) && !value.GetType().IsPrimitive)
                    {
                        if (ContainsObjectRecursive(value, targetObject, visited)) return true;
                    }
                }
                catch
                {
                    continue;
                }
            }
            return false;
        }
    }
}