using Codice.CM.SEIDInfo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public static class InspectorDataManager
{
    private enum ETypes
    {
        String, Int, Float
    }

    private static Dictionary<Type, ETypes> m_Typedictionary = new Dictionary<Type, ETypes>()
    {
        { typeof(string), ETypes.String },
        { typeof(int), ETypes.Int },
        { typeof(float), ETypes.Float }
    };

    public static void Test(ScriptableObject test)
    { }

    public static VisualElement CreateEntry(ScriptableObject _ParentSO, PropertyInfo _Property)
    {
        if (_Property.CanRead && _Property.CanWrite && _Property.IsDefined(typeof(ItemToolkitAccess), false))
        {
            VisualElement parent = new VisualElement();
            parent.style.flexDirection = FlexDirection.Row;
            Label label = new Label($"{_Property.Name}: ");
            parent.Add(label);
            TextField field = new TextField();

            if (_Property.PropertyType.IsEnum)
            {
                List<string> enumNames = Enum.GetNames(_Property.PropertyType).ToList();
                DropdownField dropdownField = new DropdownField(enumNames, Enum.GetName(_Property.PropertyType, _Property.GetValue(_ParentSO)));
                parent.Add(dropdownField);

                return parent;
            }
            else if (_Property.PropertyType.IsSubclassOf(typeof(ScriptableObject)))
            {
                List<ScriptableObject> soList = UIAssetLoader.LoadAssetsByType<ScriptableObject>();
                List<string> soNames = new List<string>();
                for (int i = 0; i < soList.Count; i++)
                {
                    if (soList[i].GetType().IsSubclassOf(_Property.PropertyType))
                    {
                        soNames.Add(soList[i].name + " (" + soList[i].GetType() + ")");
                    }
                }
                DropdownField dropdownField = new DropdownField(soNames, _Property.GetValue(_ParentSO).ToString());
                parent.Add(dropdownField);

                return parent;
            }
            else if (typeof(IDictionary).IsAssignableFrom(_Property.PropertyType))
            {
                //Type[] dictTypes = _Property.PropertyType.GetGenericArguments();
                //Type dictValType = dictTypes[1];

                //object dict = _Property.GetValue(_ParentSO);
                //if (dict is IDictionary dictionary)
                //{
                //    List<ScriptableObject> values = new List<ScriptableObject>();
                //    foreach (DictionaryEntry entry in dictionary)
                //    {
                //        values.Add(entry.Value as ScriptableObject);
                //    }
                //    InspectorList<ScriptableObject> list = new InspectorList<ScriptableObject>(values, "test", Test);
                //    parent.Add(list);
                //    return parent;
                //}
                return null;
            }
            else if (m_Typedictionary.ContainsKey(_Property.PropertyType))
            {
                switch (m_Typedictionary[_Property.PropertyType])
                {
                    case ETypes.String:

                        field.maxLength = 20;
                        field.value = _Property.GetValue(_ParentSO)?.ToString() ?? string.Empty;
                        parent.Add(field);

                        return parent;

                    case ETypes.Int:
                        field.maxLength = 5;
                        field.value = _Property.GetValue(_ParentSO)?.ToString() ?? string.Empty;
                        parent.Add(field);

                        return parent;

                    case ETypes.Float:
                        field.maxLength = 10;
                        field.value = _Property.GetValue(_ParentSO)?.ToString() ?? string.Empty;
                        parent.Add(field);

                        return parent;

                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }
}