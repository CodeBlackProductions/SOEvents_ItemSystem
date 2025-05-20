using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace ItemSystem.MainModule
{
    public static class EffectRegistryHelper
    {
        /// <summary>
        /// Recursively registers all effects in the object and its children.
        /// </summary>
        /// <param name="_Obj"></param>
        /// <param name="_RegisterEffect"></param>
        public static void RegisterAllEffectsRecursive(object _Obj, Action<SO_Item_Effect> _RegisterEffect)
        {
            if (_Obj == null) return;

            if (_Obj is IEffectModule effectModule && effectModule.Effects != null)
            {
                foreach (var effect in effectModule.Effects)
                {
                    if (effect != null)
                    {
                        _RegisterEffect(effect);
                    }
                }
            }

            var type = _Obj.GetType();

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (typeof(ScriptableObject).IsAssignableFrom(prop.PropertyType))
                {
                    var subObj = prop.GetValue(_Obj) as ScriptableObject;
                    RegisterAllEffectsRecursive(subObj, _RegisterEffect);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                {
                    var enumerable = prop.GetValue(_Obj) as IEnumerable;
                    if (enumerable != null)
                    {
                        foreach (var element in enumerable)
                        {
                            RegisterAllEffectsRecursive(element, _RegisterEffect);
                        }
                    }
                }
            }
        }
    }
}