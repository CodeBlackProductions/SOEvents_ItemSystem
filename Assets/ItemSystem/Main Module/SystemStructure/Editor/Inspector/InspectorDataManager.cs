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

    public static VisualElement CreateEntry(ScriptableObject _ParentSO, PropertyInfo _Property)
    {
        if (_Property.CanRead && _Property.CanWrite && _Property.IsDefined(typeof(ItemToolkitAccess), false))
        {
            if (ConditionalHideAttribute.ShouldShowProperty(_ParentSO, _Property))
            {
                VisualElement parent = new VisualElement();
                parent.style.flexDirection = FlexDirection.Row;
                Label label = new Label($"{_Property.Name}: ");
                parent.Add(label);
                TextField field = new TextField();

                if (_Property.PropertyType.IsEnum)
                {
                    List<string> enumNames = Enum.GetNames(_Property.PropertyType).ToList();

                    if (enumNames.Count > 0)
                    {
                        string currentEntry = Enum.GetName(_Property.PropertyType, _Property.GetValue(_ParentSO));

                        if (currentEntry != null)
                        {
                            DropdownField dropdownField = new DropdownField(enumNames, currentEntry);

                            parent.Add(dropdownField);

                            return parent;
                        }
                        else
                        {
                            currentEntry = $"Select Option";
                            enumNames.Insert(0, currentEntry);

                            DropdownField dropdownField = new DropdownField(enumNames, currentEntry);

                            parent.Add(dropdownField);

                            return parent;
                        }
                    }

                    Debug.LogWarning($"Enum entries could not be found for {_ParentSO} : {_Property.Name}");
                    return null;
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

                    if (soNames.Count > 0)
                    {
                        string currentEntry = _Property.GetValue(_ParentSO)?.ToString();

                        if (currentEntry != null)
                        {
                            DropdownField dropdownField = new DropdownField(soNames, currentEntry);

                            parent.Add(dropdownField);

                            return parent;
                        }
                        else
                        {
                            currentEntry = $"Select {_Property.PropertyType}";
                            soNames.Insert(0, currentEntry);

                            DropdownField dropdownField = new DropdownField(soNames, currentEntry);

                            parent.Add(dropdownField);

                            return parent;
                        }
                    }

                    Debug.LogWarning($"ScriptableObject entries could not be found for {_ParentSO} : {_Property.Name}");
                    return null;
                }
                else if (typeof(IDictionary).IsAssignableFrom(_Property.PropertyType))
                {
                    object dict = _Property.GetValue(_ParentSO);

                    if (_Property.PropertyType == typeof(Dictionary<string, SO_Stat>))
                    {
                        List<SO_Stat> stats = (dict as Dictionary<string, SO_Stat>).Values.ToList();

                        InspectorList<SO_Stat> statList = new InspectorList<SO_Stat>(stats, "Stats");
                        parent.Add(statList);
                        return parent;
                    }

                    Debug.LogWarning($"Could not generate InspectorList for Dictionary {_ParentSO} : {_Property.Name}");
                    return null;
                }
                else if (typeof(Array).IsAssignableFrom(_Property.PropertyType))
                {
                    if (_Property.PropertyType == typeof(SO_Item_Effect[]))
                    {
                        List<SO_Item_Effect> effects = (_Property.GetValue(_ParentSO) as SO_Item_Effect[]).ToList();

                        InspectorList<SO_Item_Effect> effectList = new InspectorList<SO_Item_Effect>(effects, "Effects");
                        parent.Add(effectList);
                        return parent;
                    }

                    if (_Property.PropertyType == typeof(SO_Class_Type[]))
                    {
                        List<SO_Class_Type> types = (_Property.GetValue(_ParentSO) as SO_Class_Type[]).ToList();

                        InspectorList<SO_Class_Type> typeList = new InspectorList<SO_Class_Type>(types, "Types");
                        parent.Add(typeList);
                        return parent;
                    }

                    Debug.LogWarning($"Could not generate InspectorList for Array {_ParentSO} : {_Property.Name}");
                    return null;
                }
                else if (typeof(IProjectile).IsAssignableFrom(_Property.PropertyType))
                {
                    List<GameObject> projectileList = UIAssetLoader.LoadAssetsByType<GameObject>().Where(asset => asset.GetComponent<IProjectile>() != null).ToList();
                    List<string> projectileNames = new List<string>();
                    for (int i = 0; i < projectileList.Count; i++)
                    {
                        projectileNames.Add(projectileList[i].name);
                    }

                    if (projectileNames.Count > 0)
                    {
                        string currentEntry = _Property.GetValue(_ParentSO)?.ToString();

                        if (currentEntry != null)
                        {
                            DropdownField dropdownField = new DropdownField(projectileNames, currentEntry);

                            parent.Add(dropdownField);

                            return parent;
                        }
                        else
                        {
                            currentEntry = "Select Projectile";
                            projectileNames.Insert(0, currentEntry);

                            DropdownField dropdownField = new DropdownField(projectileNames, currentEntry);

                            parent.Add(dropdownField);

                            return parent;
                        }
                    }

                    Debug.LogWarning($"Projectile Class entries could not be found for {_ParentSO} : {_Property.Name}");
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

            return null;
        }
        else
        {
            return null;
        }
    }
}