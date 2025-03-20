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

    public static VisualElement CreateEntry(ScriptableObject _ParentSO, PropertyInfo _Property, InspectorPanel _ParentPanel, Action _InspectorValueChangeCallback)
    {
        if (_Property.CanRead && _Property.CanWrite && _Property.IsDefined(typeof(ItemToolkitAccess), false))
        {
            if (ConditionalHideAttribute.ShouldShowProperty(_ParentSO, _Property))
            {
                VisualElement parent = new VisualElement();
                parent.style.flexDirection = FlexDirection.Row;
                parent.style.alignSelf = Align.FlexStart;
                parent.style.paddingBottom = 10;
                Label label = new Label($"{_Property.Name}: ");
                parent.Add(label);

                if (_Property.PropertyType.IsEnum)
                {
                    List<string> enumNames = Enum.GetNames(_Property.PropertyType).ToList();

                    if (enumNames.Count > 0)
                    {
                        string currentEntry = Enum.GetName(_Property.PropertyType, _Property.GetValue(_ParentSO));

                        if (currentEntry == null)
                        {
                            currentEntry = $"Select Option";
                            enumNames.Insert(0, currentEntry);
                        }

                        DropdownField dropdownField = new DropdownField(enumNames, currentEntry);

                        dropdownField.RegisterValueChangedCallback(c =>
                        {
                            _Property.SetValue(_ParentSO, Enum.Parse(_Property.PropertyType, c.newValue));
                            _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                            _InspectorValueChangeCallback?.Invoke();
                        });

                        parent.Add(dropdownField);

                        return parent;
                    }

                    Debug.LogWarning($"Enum entries could not be found for {_ParentSO} : {_Property.Name}");
                    return null;
                }
                else if (_Property.PropertyType.IsSubclassOf(typeof(ScriptableObject)))
                {
                    List<ScriptableObject> soList = ItemEditor_AssetLoader.LoadAssetsByType<ScriptableObject>();
                    if (soList == null)
                    {
                        soList = new List<ScriptableObject>();
                    }
                    List<string> soNames = new List<string>();
                    for (int i = 0; i < soList.Count; i++)
                    {
                        if (soList[i].GetType().IsSubclassOf(_Property.PropertyType))
                        {
                            soNames.Add($"{soList[i].name} ({soList[i].GetType().Name})");
                        }
                    }

                    if (soNames.Count > 0)
                    {
                        string currentEntry = _Property.GetValue(_ParentSO)?.ToString();

                        if (currentEntry == null)
                        {
                            currentEntry = $"Select {_Property.PropertyType}";
                            soNames.Insert(0, currentEntry);
                        }

                        DropdownField dropdownField = new DropdownField(soNames, currentEntry);

                        dropdownField.RegisterValueChangedCallback(c =>
                        {
                            _Property.SetValue(
                                _ParentSO, soList.Find(so =>
                                $"{so.name} ({so.GetType().Name})" == c.newValue && _Property.PropertyType.IsAssignableFrom(so.GetType())
                                )
                            );
                            _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                            _InspectorValueChangeCallback?.Invoke();
                        });

                        parent.Add(dropdownField);

                        return parent;
                    }

                    Debug.LogWarning($"ScriptableObject entries could not be found for {_ParentSO} : {_Property.Name}");
                    return null;
                }
                else if (typeof(IDictionary).IsAssignableFrom(_Property.PropertyType))
                {
                    object dict = _Property.GetValue(_ParentSO);

                    if (_Property.PropertyType == typeof(Dictionary<string, SO_Stat>))
                    {
                        Dictionary<string, SO_Stat> dictionary = dict as Dictionary<string, SO_Stat>;
                        if (dictionary == null)
                        {
                            dictionary = new Dictionary<string, SO_Stat>();
                        }
                        InspectorList<SO_Stat> statList = new InspectorList<SO_Stat>(dictionary, "Stats");

                        statList.ItemAddCallback += (newItem) =>
                        {
                            dictionary.Add(newItem.GetStatName(), newItem);
                            _Property.SetValue(_ParentSO, dictionary);
                            _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                            _InspectorValueChangeCallback?.Invoke();
                        };

                        statList.ItemRemoveCallback += (removeItem) =>
                        {
                            dictionary.Remove(removeItem.GetStatName());
                            _Property.SetValue(_ParentSO, dictionary);
                            _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                            _InspectorValueChangeCallback?.Invoke();
                        };

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
                        SO_Item_Effect[] array = _Property.GetValue(_ParentSO) as SO_Item_Effect[];
                        if (array == null)
                        {
                            array = new SO_Item_Effect[0];
                        }
                        InspectorList<SO_Item_Effect> effectList = new InspectorList<SO_Item_Effect>(array, "Effects");

                        effectList.ItemAddCallback += (newItem) =>
                        {
                            SO_Item_Effect[] newArray = new SO_Item_Effect[array.Length + 1];
                            Array.Copy(array, newArray, array.Length);
                            newArray[newArray.Length - 1] = newItem;

                            array = newArray;

                            _Property.SetValue(_ParentSO, newArray);

                            _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                            _InspectorValueChangeCallback?.Invoke();
                        };

                        effectList.ItemRemoveCallback += (newItem) =>
                        {
                            SO_Item_Effect[] newArray = new SO_Item_Effect[array.Length - 1];
                            int index = Array.IndexOf(array, newItem);

                            if (index >= 0)
                            {
                                Array.Copy(array, 0, newArray, 0, index);
                                Array.Copy(array, index + 1, newArray, index, array.Length - index - 1);
                            }

                            array = newArray;

                            _Property.SetValue(_ParentSO, newArray);

                            _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                            _InspectorValueChangeCallback?.Invoke();
                        };

                        parent.Add(effectList);
                        return parent;
                    }

                    if (_Property.PropertyType == typeof(SO_Class_Type[]))
                    {
                        SO_Class_Type[] array = _Property.GetValue(_ParentSO) as SO_Class_Type[];
                        InspectorList<SO_Class_Type> typeList = new InspectorList<SO_Class_Type>(_Property.GetValue(_ParentSO) as SO_Class_Type[], "Types");

                        typeList.ItemAddCallback += (newItem) =>
                        {
                            SO_Class_Type[] newArray = new SO_Class_Type[array.Length + 1];
                            Array.Copy(array, newArray, array.Length);
                            newArray[newArray.Length - 1] = newItem;

                            array = newArray;

                            _Property.SetValue(_ParentSO, newArray);

                            _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                            _InspectorValueChangeCallback?.Invoke();
                        };

                        typeList.ItemRemoveCallback += (newItem) =>
                        {
                            SO_Class_Type[] newArray = new SO_Class_Type[array.Length - 1];
                            int index = Array.IndexOf(array, newItem);

                            if (index >= 0)
                            {
                                Array.Copy(array, 0, newArray, 0, index);
                                Array.Copy(array, index + 1, newArray, index, array.Length - index - 1);
                            }

                            array = newArray;

                            _Property.SetValue(_ParentSO, newArray);

                            _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                            _InspectorValueChangeCallback?.Invoke();
                        };

                        parent.Add(typeList);
                        return parent;
                    }

                    Debug.LogWarning($"Could not generate InspectorList for Array {_ParentSO} : {_Property.Name}");
                    return null;
                }
                else if (typeof(IProjectile).IsAssignableFrom(_Property.PropertyType))
                {
                    List<GameObject> projectileList = ItemEditor_AssetLoader.LoadAssetsByType<GameObject>().Where(asset => asset.GetComponent<IProjectile>() != null).ToList();
                    if (projectileList == null)
                    {
                        projectileList = new List<GameObject>();
                    }
                    List<string> projectileNames = new List<string>();
                    for (int i = 0; i < projectileList.Count; i++)
                    {
                        projectileNames.Add(projectileList[i].name);
                    }

                    if (projectileNames.Count > 0)
                    {
                        string currentEntry = (_Property.GetValue(_ParentSO) as IProjectile)?.Name;

                        if (currentEntry == null || !projectileNames.Contains(currentEntry))
                        {
                            currentEntry = "Select Projectile";
                            projectileNames.Insert(0, currentEntry);
                        }

                        DropdownField dropdownField = new DropdownField(projectileNames, currentEntry);

                        dropdownField.RegisterValueChangedCallback(c =>
                        {
                            _Property.SetValue(_ParentSO, projectileList.Find(obj =>
                                obj.name == c.newValue && obj.GetComponent<IProjectile>() != null)
                                .GetComponent<IProjectile>());

                            _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                            _InspectorValueChangeCallback?.Invoke();
                        });

                        parent.Add(dropdownField);

                        return parent;
                    }

                    Debug.LogWarning($"Projectile Class entries could not be found for {_ParentSO} : {_Property.Name}");
                    return null;
                }
                else if (m_Typedictionary.ContainsKey(_Property.PropertyType) && _Property.Name != "TypeIndex")
                {
                    TextField field = new TextField();
                    switch (m_Typedictionary[_Property.PropertyType])
                    {
                        case ETypes.String:

                            field.maxLength = 20;
                            field.value = _Property.GetValue(_ParentSO)?.ToString() ?? string.Empty;
                            parent.Add(field);

                            field.RegisterValueChangedCallback(t =>
                            {
                                _Property.SetValue(_ParentSO, t.newValue);
                                _InspectorValueChangeCallback?.Invoke();
                            });

                            return parent;

                        case ETypes.Int:
                            field.maxLength = 5;
                            field.value = _Property.GetValue(_ParentSO)?.ToString() ?? string.Empty;
                            parent.Add(field);

                            field.RegisterValueChangedCallback(t =>
                            {
                                if (int.TryParse(t.newValue, out int result))
                                {
                                    _Property.SetValue(_ParentSO, result);
                                }
                                else
                                {
                                    Debug.LogWarning("Invalid input");
                                }
                            });

                            return parent;

                        case ETypes.Float:
                            field.maxLength = 10;
                            field.value = _Property.GetValue(_ParentSO)?.ToString() ?? string.Empty;
                            parent.Add(field);

                            field.RegisterValueChangedCallback(t =>
                            {
                                if (float.TryParse(t.newValue, out float result))
                                {
                                    _Property.SetValue(_ParentSO, result);
                                }
                                else
                                {
                                    Debug.LogWarning("Invalid input");
                                }
                            });

                            return parent;

                        default:
                            return null;
                    }
                }
                else if (_Property.Name == "TypeIndex")
                {
                    parent.Clear();
                    label = new Label("Type:");
                    parent.Add(label);

                    PropertyInfo classProp = _ParentSO.GetType().GetProperty("Class");

                    if (classProp == null)
                    {
                        Label noTypes = new Label("No types declared in class");
                        parent.Add(noTypes);
                        return parent;
                    }
                    SO_Item_Class itemClass = classProp.GetValue(_ParentSO) as SO_Item_Class;

                    if (itemClass == null)
                    {
                        Label noTypes = new Label("No types declared in class");
                        parent.Add(noTypes);
                        return parent;
                    }

                    PropertyInfo typeProp = itemClass.GetType().GetProperty("Types");

                    if (typeProp == null)
                    {
                        Label noTypes = new Label("No types declared in class");
                        parent.Add(noTypes);
                        return parent;
                    }

                    List<SO_Class_Type> typeList = new List<SO_Class_Type>(typeProp.GetValue(itemClass) as SO_Class_Type[]);

                    if (typeList.Count > 0)
                    {
                        List<string> soNames = new List<string>();
                        for (int i = 0; i < typeList.Count; i++)
                        {
                            soNames.Add($"{typeList[i].name} ({typeList[i].GetType().Name})");
                        }

                        int currentEntry = (int)_Property.GetValue(_ParentSO);
                        string currentEntryName = soNames[currentEntry];

                        DropdownField dropdownField = new DropdownField(soNames, currentEntryName);

                        dropdownField.RegisterValueChangedCallback(c =>
                        {
                            _Property.SetValue(
                                _ParentSO, typeList.FindIndex(so =>
                                $"{so.name} ({so.GetType().Name})" == c.newValue)
                            );
                            _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                            _InspectorValueChangeCallback?.Invoke();
                        });

                        parent.Add(dropdownField);

                        return parent;
                    }
                    else
                    {
                        Label noTypes = new Label("No types declared in class");
                        parent.Add(noTypes);
                        return parent;
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