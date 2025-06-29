using ItemSystem.MainModule;
using ItemSystem.SubModules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Manages the data for the inspector. This includes creating the different UI elements based on object type.
    /// </summary>
    public static class InspectorDataManager
    {
        private enum ETypes
        {
            String, Int, Float, Bool, Vector2, Vector3, Color
        }

        private static Dictionary<System.Type, ETypes> m_Typedictionary = new Dictionary<System.Type, ETypes>()
        {
            { typeof(string), ETypes.String },
            { typeof(int), ETypes.Int },
            { typeof(float), ETypes.Float },
            { typeof(bool), ETypes.Bool },
            { typeof(Vector2), ETypes.Vector2 },
            { typeof(Vector3), ETypes.Vector3 },
            { typeof(Color), ETypes.Color }
        };

        public static VisualElement CreateEntry(
            ScriptableObject _ParentSO,
            PropertyInfo _Property,
            InspectorPanel _ParentPanel,
            Action<bool> _InspectorValueChangeCallback)
        {
            if (_Property.CanRead && _Property.CanWrite && _Property.IsDefined(typeof(ItemToolkitAccess), false))
            {
                if (ConditionalHideAttribute.ShouldShowProperty(_ParentSO, _Property))
                {
                    VisualElement uiParent = new VisualElement();
                    uiParent.style.flexDirection = FlexDirection.Row;
                    uiParent.style.alignSelf = Align.FlexStart;
                    uiParent.style.paddingBottom = 10;
                    Label label = new Label($"{_Property.Name}: ");
                    uiParent.Add(label);

                    uiParent.tooltip = _Property.GetAttribute<TooltipAttribute>()?.tooltip;

                    if (_Property.PropertyType.IsEnum)
                    {
                        return CreateUIforEnum(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent);
                    }
                    else if (_Property.PropertyType.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        return CreateUIforSO(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent);
                    }
                    else if (typeof(IDictionary).IsAssignableFrom(_Property.PropertyType))
                    {
                        return CreateUIforDictionary(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent);
                    }
                    else if (typeof(Array).IsAssignableFrom(_Property.PropertyType))
                    {
                        return CreateUIforArray(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent);
                    }
                    else if (typeof(GameObject).IsAssignableFrom(_Property.PropertyType))
                    {
                        return CreateUIforProjectiles(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent);
                    }
                    else if (m_Typedictionary.ContainsKey(_Property.PropertyType) && _Property.Name != "TypeIndex")
                    {
                        return CreateUIforBasicDataTypes(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent);
                    }
                    else if (_Property.Name == "TypeIndex")
                    {
                        return CreateUIforTypeSelection(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent);
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

        #region InternalMethods

        private static VisualElement CreateUIforEnum(
           ScriptableObject _ParentSO,
           PropertyInfo _Property,
           InspectorPanel _ParentPanel,
           Action<bool> _InspectorValueChangeCallback,
           VisualElement _UIParent)
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
                    EditorUtility.SetDirty(_ParentSO);
                    AssetDatabase.SaveAssets();
                    _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                    _InspectorValueChangeCallback?.Invoke(true);
                });

                _UIParent.Add(dropdownField);

                return _UIParent;
            }

            Debug.LogWarning($"Enum entries could not be found for {_ParentSO} : {_Property.Name}");
            return null;
        }

        private static VisualElement CreateUIforSO(
            ScriptableObject _ParentSO,
            PropertyInfo _Property,
            InspectorPanel _ParentPanel,
            Action<bool> _InspectorValueChangeCallback,
            VisualElement _UIParent)
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
                    soNames.Add($"{(soList[i] as IItemModule).ModuleName} ({soList[i].GetType().Name})");
                }
            }

            if (soNames.Count > 0)
            {
                var propertyVal = _Property.GetValue(_ParentSO);
                string currentEntry = null;

                if (propertyVal != null)
                {
                    currentEntry = $"{(_Property.GetValue(_ParentSO) as IItemModule).ModuleName} ({_Property.GetValue(_ParentSO).GetType().Name})";
                }

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
                        $"{(so as IItemModule).ModuleName} ({so.GetType().Name})" == c.newValue && _Property.PropertyType.IsAssignableFrom(so.GetType())
                        )
                    );
                    EditorUtility.SetDirty(_ParentSO);
                    AssetDatabase.SaveAssets();
                    _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                    _InspectorValueChangeCallback?.Invoke(true);
                });

                _UIParent.Add(dropdownField);

                return _UIParent;
            }

            Debug.LogWarning($"ScriptableObject entries could not be found for {_ParentSO} : {_Property.Name}");
            return null;
        }

        private static VisualElement CreateUIforDictionary(
            ScriptableObject _ParentSO,
            PropertyInfo _Property,
            InspectorPanel _ParentPanel,
            Action<bool> _InspectorValueChangeCallback,
            VisualElement _UIParent)
        {
            object dict = _Property.GetValue(_ParentSO);

            if (_Property.PropertyType == typeof(Dictionary<string, SO_Stat>))
            {
                List<SO_Stat> allStats = ItemEditor_AssetLoader.LoadAssetsByType<SO_Stat>();
                List<string> allTargetUserStats = new List<string>();

                allStats.ForEach(so =>
                {
                    if (so.GetType().IsSubclassOf(typeof(SO_Stat)) && !allTargetUserStats.Contains(so.TargetUserStat))
                    {
                        allTargetUserStats.Add(so.TargetUserStat);
                    }
                });

                Dictionary<string, SO_Stat> statDictionary = dict as Dictionary<string, SO_Stat>;
                if (statDictionary == null)
                {
                    statDictionary = new Dictionary<string, SO_Stat>();
                }

                List<string> selectedStats = new List<string>();
                foreach (KeyValuePair<string, SO_Stat> entry in statDictionary)
                {
                    if (!selectedStats.Contains(entry.Value.TargetUserStat))
                    {
                        selectedStats.Add(entry.Value.TargetUserStat);
                    }
                }

                VisualElement container = new VisualElement();
                VisualElement selectedList = new VisualElement();
                selectedList.style.flexDirection = FlexDirection.Column;

                List<string> availableStats = allTargetUserStats.Except(selectedStats).ToList();
                DropdownField dropdown;
                if (availableStats.Count > 0)
                {
                    dropdown = new DropdownField("", availableStats, availableStats.FirstOrDefault());
                }
                else
                {
                    dropdown = new DropdownField("", new List<string> { "No available stats" }, "No available stats");
                    dropdown.SetEnabled(false);
                }

                Action updateUI = null;
                updateUI = () =>
                {
                    selectedList.Clear();
                    foreach (var stat in selectedStats)
                    {
                        var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                        row.Add(new Label(stat));

                        var matchingStats = allStats
                            .Where(s => s.TargetUserStat == stat)
                            .ToList();

                        List<string> soNames = matchingStats
                            .Select(s => $"{s.ModuleName} ({s.GetType().Name})")
                            .ToList();

                        SO_Stat current = statDictionary.FirstOrDefault(x => x.Value.TargetUserStat == stat).Value;
                        string currentName = current != null ? $"{current.ModuleName} ({current.GetType().Name})" : soNames.FirstOrDefault();

                        DropdownField statDropdown = new DropdownField("", soNames, currentName);
                        statDropdown.RegisterValueChangedCallback(c =>
                        {
                            SO_Stat selectedSO = matchingStats.FirstOrDefault(s => $"{s.ModuleName} ({s.GetType().Name})" == c.newValue);
                            if (selectedSO != null)
                            {
                                string oldKey = statDictionary.FirstOrDefault(x => x.Value.TargetUserStat == stat).Key;
                                if (!string.IsNullOrEmpty(oldKey))
                                {
                                    statDictionary.Remove(oldKey);
                                }

                                statDictionary[selectedSO.StatName] = selectedSO;
                                _Property.SetValue(_ParentSO, statDictionary);
                                EditorUtility.SetDirty(_ParentSO);
                                AssetDatabase.SaveAssets();
                                _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                                _InspectorValueChangeCallback?.Invoke(true);

                                updateUI();
                            }
                        });

                        row.Add(statDropdown);

                        var removeButton = new Button(() =>
                        {
                            selectedStats.Remove(stat);
                            dropdown.choices = allTargetUserStats.Except(selectedStats).ToList();
                            dropdown.value = dropdown.choices.FirstOrDefault();
                            updateUI();

                            string toRemove = statDictionary.FirstOrDefault(x => x.Value.TargetUserStat == stat).Key;
                            statDictionary.Remove(toRemove);
                            _Property.SetValue(_ParentSO, statDictionary);
                            EditorUtility.SetDirty(_ParentSO);
                            AssetDatabase.SaveAssets();
                            _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                            _InspectorValueChangeCallback?.Invoke(true);
                        })
                        { text = "Remove" };
                        row.Add(removeButton);

                        selectedList.Add(row);
                    }
                };

                Button addButton = new Button(() =>
                {
                    string selected = dropdown.value;
                    if (!string.IsNullOrEmpty(selected) && !selectedStats.Contains(selected))
                    {
                        selectedStats.Add(selected);
                        dropdown.choices = allTargetUserStats.Except(selectedStats).ToList();
                        dropdown.value = dropdown.choices.FirstOrDefault();
                        updateUI();

                        SO_Stat newItem = allStats.FirstOrDefault(so => so.TargetUserStat == selected);
                        statDictionary.Add(newItem.StatName, newItem);
                        _Property.SetValue(_ParentSO, statDictionary);
                        EditorUtility.SetDirty(_ParentSO);
                        AssetDatabase.SaveAssets();
                        _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                        _InspectorValueChangeCallback?.Invoke(true);
                    }
                })
                { text = "Add" };

                if (!dropdown.enabledSelf)
                {
                    addButton.SetEnabled(false);
                }

                VisualElement SelectionContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                SelectionContainer.Add(dropdown);
                SelectionContainer.Add(addButton);

                container.Add(SelectionContainer);
                container.Add(selectedList);

                updateUI();

                _UIParent.Add(container);
                return _UIParent;
            }

            Debug.LogWarning($"Could not generate InspectorList for Dictionary {_ParentSO} : {_Property.Name}");
            return null;
        }

        private static VisualElement CreateUIforArray(
            ScriptableObject _ParentSO,
            PropertyInfo _Property,
            InspectorPanel _ParentPanel,
            Action<bool> _InspectorValueChangeCallback,
            VisualElement _UIParent)
        {
            if (_Property.PropertyType == typeof(SO_Item_Effect[]))
            {
                InspectorList<SO_Item_Effect> effectList = ConvertArrayToInspectorList<SO_Item_Effect>(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, "Effects", true);

                _UIParent.Add(effectList);
                return _UIParent;
            }

            if (_Property.PropertyType == typeof(SO_Class_Type[]))
            {
                InspectorList<SO_Class_Type> typeList = ConvertArrayToInspectorList<SO_Class_Type>(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, "Types", true);

                _UIParent.Add(typeList);
                return _UIParent;
            }

            if (_Property.PropertyType == typeof(SO_Item_Class[]))
            {
                InspectorList<SO_Item_Class> classList = ConvertArrayToInspectorList<SO_Item_Class>(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, "Classes", true);

                _UIParent.Add(classList);
                return _UIParent;
            }

            if (_Property.PropertyType == typeof(SO_Tag[]))
            {
                InspectorList<SO_Tag> classList = ConvertArrayToInspectorList<SO_Tag>(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, "Tags", true);

                _UIParent.Add(classList);
                return _UIParent;
            }

            if (_Property.PropertyType == typeof(SO_Stat[]))
            {
                InspectorList<SO_Stat> statList = ConvertArrayToInspectorList<SO_Stat>(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, "Stats", true);

                _UIParent.Add(statList);
                return _UIParent;
            }

            if (_Property.PropertyType == typeof(SO_ToolTip[]))
            {
                InspectorList<SO_ToolTip> toolTipList = ConvertArrayToInspectorList<SO_ToolTip>(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, "ToolTips", true);

                _UIParent.Add(toolTipList);
                return _UIParent;
            }

            Debug.LogWarning($"Could not generate InspectorList for Array {_ParentSO} : {_Property.Name}");
            return null;
        }

        private static VisualElement CreateUIforProjectiles(
           ScriptableObject _ParentSO,
           PropertyInfo _Property,
           InspectorPanel _ParentPanel,
           Action<bool> _InspectorValueChangeCallback,
           VisualElement _UIParent)
        {
            List<GameObject> projectileList = ItemEditor_AssetLoader.LoadAssetsByType<GameObject>().Where(asset => asset.GetComponent<IProjectile>() != null).ToList();
            if (projectileList == null)
            {
                projectileList = new List<GameObject>();
            }
            List<string> projectileNames = new List<string>();
            for (int i = 0; i < projectileList.Count; i++)
            {
                projectileNames.Add(projectileList[i].GetComponent<IProjectile>()?.ProjectileName);
            }

            if (projectileNames.Count > 0)
            {
                object propertyVal = _Property.GetValue(_ParentSO);
                string currentEntry = null;

                if (propertyVal is UnityEngine.Object unityObj && unityObj != null)
                {
                    currentEntry = (propertyVal as GameObject)?.GetComponent<IProjectile>().ProjectileName;
                }

                if (currentEntry == null || !projectileNames.Contains(currentEntry))
                {
                    currentEntry = "Select Projectile";
                    projectileNames.Insert(0, currentEntry);
                }

                DropdownField dropdownField = new DropdownField(projectileNames, currentEntry);

                dropdownField.RegisterValueChangedCallback(c =>
                {
                    _Property.SetValue(_ParentSO, projectileList.Find(obj =>
                        obj.GetComponent<IProjectile>() != null && obj.GetComponent<IProjectile>()?.ProjectileName == c.newValue));

                    EditorUtility.SetDirty(_ParentSO);
                    AssetDatabase.SaveAssets();

                    _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                    _InspectorValueChangeCallback?.Invoke(true);
                });

                _UIParent.Add(dropdownField);

                return _UIParent;
            }

            Debug.LogWarning($"Projectile Class entries could not be found for {_ParentSO} : {_Property.Name}");
            return null;
        }

        private static VisualElement CreateUIforBasicDataTypes(
           ScriptableObject _ParentSO,
           PropertyInfo _Property,
           InspectorPanel _ParentPanel,
           Action<bool> _InspectorValueChangeCallback,
           VisualElement _UIParent)
        {
            TextField field = new TextField();
            TextField field2 = new TextField();
            TextField field3 = new TextField();

            _UIParent.tooltip = _Property.GetAttribute<TooltipAttribute>()?.tooltip;

            switch (m_Typedictionary[_Property.PropertyType])
            {
                case ETypes.String:

                    field.value = _Property.GetValue(_ParentSO)?.ToString() ?? string.Empty;
                    _UIParent.Add(field);

                    field.RegisterValueChangedCallback(t =>
                    {
                        _Property.SetValue(_ParentSO, t.newValue);
                        EditorUtility.SetDirty(_ParentSO);
                        AssetDatabase.SaveAssets();
                        _InspectorValueChangeCallback?.Invoke(true);
                    });

                    field.RegisterCallback<FocusOutEvent>(t =>
                    _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback)
                    );

                    return _UIParent;

                case ETypes.Int:

                    field.value = _Property.GetValue(_ParentSO)?.ToString() ?? string.Empty;
                    _UIParent.Add(field);

                    field.RegisterValueChangedCallback(t =>
                    {
                        if (int.TryParse(t.newValue, out int result))
                        {
                            _Property.SetValue(_ParentSO, result);
                            EditorUtility.SetDirty(_ParentSO);
                            AssetDatabase.SaveAssets();
                        }
                        else
                        {
                            Debug.LogWarning("Invalid input");
                        }
                    });

                    field.RegisterCallback<FocusOutEvent>(t =>
                   _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback)
                   );

                    return _UIParent;

                case ETypes.Float:

                    field.value = _Property.GetValue(_ParentSO)?.ToString() ?? string.Empty;
                    _UIParent.Add(field);

                    field.RegisterValueChangedCallback(t =>
                    {
                        if (float.TryParse(t.newValue, out float result))
                        {
                            _Property.SetValue(_ParentSO, result);
                            EditorUtility.SetDirty(_ParentSO);
                            AssetDatabase.SaveAssets();
                        }
                        else
                        {
                            Debug.LogWarning("Invalid input");
                        }
                    });

                    field.RegisterCallback<FocusOutEvent>(t =>
                   _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback)
                   );

                    return _UIParent;

                case ETypes.Bool:

                    Toggle boolField = new Toggle();
                    boolField.value = (bool)_Property.GetValue(_ParentSO);

                    _UIParent.Add(boolField);

                    boolField.RegisterValueChangedCallback(t =>
                    {
                        _Property.SetValue(_ParentSO, t.newValue);
                        EditorUtility.SetDirty(_ParentSO);
                        AssetDatabase.SaveAssets();
                        _InspectorValueChangeCallback?.Invoke(true);
                        _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                    });

                    return _UIParent;

                case ETypes.Vector2:

                    field.value = ((Vector2)_Property.GetValue(_ParentSO)).x.ToString() ?? string.Empty;

                    field2.value = ((Vector2)_Property.GetValue(_ParentSO)).y.ToString() ?? string.Empty;

                    _UIParent.Add(field);
                    _UIParent.Add(field2);

                    field.RegisterValueChangedCallback(t =>
                    {
                        if (float.TryParse(t.newValue, out float result))
                        {
                            if (float.TryParse(field2.value, out float result2))
                            {
                                Vector2 newValue = new Vector2(result, result2);
                                _Property.SetValue(_ParentSO, newValue);
                                EditorUtility.SetDirty(_ParentSO);
                                AssetDatabase.SaveAssets();
                            }
                            else
                            {
                                Debug.LogWarning("Invalid input");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Invalid input");
                        }
                    });

                    field2.RegisterValueChangedCallback(t =>
                    {
                        if (float.TryParse(t.newValue, out float result2))
                        {
                            if (float.TryParse(field.value, out float result))
                            {
                                Vector2 newValue = new Vector2(result, result2);
                                _Property.SetValue(_ParentSO, newValue);
                                EditorUtility.SetDirty(_ParentSO);
                                AssetDatabase.SaveAssets();
                            }
                            else
                            {
                                Debug.LogWarning("Invalid input");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Invalid input");
                        }
                    });

                    field.RegisterCallback<FocusOutEvent>(t =>
                   _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback)
                   );

                    field2.RegisterCallback<FocusOutEvent>(t =>
                 _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback)
                 );

                    return _UIParent;

                case ETypes.Vector3:

                    field.value = ((Vector3)_Property.GetValue(_ParentSO)).x.ToString() ?? string.Empty;

                    field2.value = ((Vector3)_Property.GetValue(_ParentSO)).y.ToString() ?? string.Empty;

                    field3.value = ((Vector3)_Property.GetValue(_ParentSO)).z.ToString() ?? string.Empty;

                    _UIParent.Add(field);
                    _UIParent.Add(field2);
                    _UIParent.Add(field3);

                    field.RegisterValueChangedCallback(t =>
                    {
                        if (float.TryParse(t.newValue, out float result))
                        {
                            if (float.TryParse(field2.value, out float result2))
                            {
                                if (float.TryParse(field3.value, out float result3))
                                {
                                    Vector3 newValue = new Vector3(result, result2, result3);
                                    _Property.SetValue(_ParentSO, newValue);
                                    EditorUtility.SetDirty(_ParentSO);
                                    AssetDatabase.SaveAssets();
                                }
                                else
                                {
                                    Debug.LogWarning("Invalid input");
                                }
                            }
                            else
                            {
                                Debug.LogWarning("Invalid input");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Invalid input");
                        }
                    });

                    field2.RegisterValueChangedCallback(t =>
                    {
                        if (float.TryParse(t.newValue, out float result2))
                        {
                            if (float.TryParse(field.value, out float result))
                            {
                                if (float.TryParse(field3.value, out float result3))
                                {
                                    Vector3 newValue = new Vector3(result, result2, result3);
                                    _Property.SetValue(_ParentSO, newValue);
                                    EditorUtility.SetDirty(_ParentSO);
                                    AssetDatabase.SaveAssets();
                                }
                                else
                                {
                                    Debug.LogWarning("Invalid input");
                                }
                            }
                            else
                            {
                                Debug.LogWarning("Invalid input");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Invalid input");
                        }
                    });

                    field3.RegisterValueChangedCallback(t =>
                    {
                        if (float.TryParse(t.newValue, out float result3))
                        {
                            if (float.TryParse(field.value, out float result))
                            {
                                if (float.TryParse(field2.value, out float result2))
                                {
                                    Vector3 newValue = new Vector3(result, result2, result3);
                                    _Property.SetValue(_ParentSO, newValue);
                                    EditorUtility.SetDirty(_ParentSO);
                                    AssetDatabase.SaveAssets();
                                }
                                else
                                {
                                    Debug.LogWarning("Invalid input");
                                }
                            }
                            else
                            {
                                Debug.LogWarning("Invalid input");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Invalid input");
                        }
                    });

                    field.RegisterCallback<FocusOutEvent>(t =>
                   _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback)
                   );

                    field2.RegisterCallback<FocusOutEvent>(t =>
                    _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback)
                    );

                    field3.RegisterCallback<FocusOutEvent>(t =>
                    _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback)
                    );

                    return _UIParent;

                case ETypes.Color:

                    ColorField colorField = new ColorField();
                    colorField.value = (Color)_Property.GetValue(_ParentSO);

                    _UIParent.Add(colorField);

                    colorField.RegisterCallback<FocusOutEvent>(t =>
                    {
                        _Property.SetValue(_ParentSO, colorField.value);
                        EditorUtility.SetDirty(_ParentSO);
                        AssetDatabase.SaveAssets();
                        _InspectorValueChangeCallback?.Invoke(true);

                        _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                    });

                    return _UIParent;

                default:
                    return null;
            }
        }

        private static VisualElement CreateUIforTypeSelection(
           ScriptableObject _ParentSO,
           PropertyInfo _Property,
           InspectorPanel _ParentPanel,
           Action<bool> _InspectorValueChangeCallback,
           VisualElement _UIParent)
        {
            _UIParent.Clear();
            Label label = new Label("Type:");
            _UIParent.Add(label);

            PropertyInfo classProp = _ParentSO.GetType().GetProperty("Class");

            if (classProp == null)
            {
                Label noTypes = new Label("No types declared in class");
                _UIParent.Add(noTypes);
                return _UIParent;
            }
            SO_Item_Class itemClass = classProp.GetValue(_ParentSO) as SO_Item_Class;

            if (itemClass == null)
            {
                Label noTypes = new Label("No types declared in class");
                _UIParent.Add(noTypes);
                return _UIParent;
            }

            PropertyInfo typeProp = itemClass.GetType().GetProperty("Types");

            if (typeProp == null)
            {
                Label noTypes = new Label("No types declared in class");
                _UIParent.Add(noTypes);
                return _UIParent;
            }

            List<SO_Class_Type> typeList = new List<SO_Class_Type>(typeProp.GetValue(itemClass) as SO_Class_Type[]);

            if (typeList.Count > 0)
            {
                List<string> soNames = new List<string>();
                for (int i = 0; i < typeList.Count; i++)
                {
                    soNames.Add($"{(typeList[i] as IItemModule).ModuleName} ({typeList[i].GetType().Name})");
                }

                int currentEntry = (int)_Property.GetValue(_ParentSO);
                string currentEntryName = soNames[currentEntry];

                DropdownField dropdownField = new DropdownField(soNames, currentEntryName);

                dropdownField.RegisterValueChangedCallback(c =>
                {
                    _Property.SetValue(
                        _ParentSO, typeList.FindIndex(so =>
                        $"{(so as IItemModule).ModuleName} ({so.GetType().Name})" == c.newValue)
                    );
                    EditorUtility.SetDirty(_ParentSO);
                    AssetDatabase.SaveAssets();
                    _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                    _InspectorValueChangeCallback?.Invoke(true);
                });

                _UIParent.Add(dropdownField);

                return _UIParent;
            }
            else
            {
                Label noTypes = new Label("No types declared in class");
                _UIParent.Add(noTypes);
                return _UIParent;
            }
        }

        private static InspectorList<T> ConvertArrayToInspectorList<T>(
            ScriptableObject _ParentSO,
            PropertyInfo _Property,
            InspectorPanel _ParentPanel,
            Action<bool> _InspectorValueChangeCallback,
            string _Title,
            bool _ShowAddAndRemove) where T : ScriptableObject
        {
            T[] array = _Property.GetValue(_ParentSO) as T[];
            if (array == null)
            {
                array = new T[0];
            }
            InspectorList<T> list = new InspectorList<T>(array, null, _Title, _ShowAddAndRemove);

            list.ItemAddCallback += (newItem) =>
            {
                T[] newArray = new T[array.Length + 1];
                Array.Copy(array, newArray, array.Length);
                newArray[newArray.Length - 1] = newItem;

                array = newArray;

                _Property.SetValue(_ParentSO, newArray);

                EditorUtility.SetDirty(_ParentSO);
                AssetDatabase.SaveAssets();

                _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                _InspectorValueChangeCallback?.Invoke(true);
            };

            list.ItemRemoveCallback += (newItem) =>
            {
                T[] newArray = new T[array.Length - 1];
                int index = Array.IndexOf(array, newItem);

                if (index >= 0)
                {
                    Array.Copy(array, 0, newArray, 0, index);
                    Array.Copy(array, index + 1, newArray, index, array.Length - index - 1);
                }

                array = newArray;

                _Property.SetValue(_ParentSO, newArray);

                EditorUtility.SetDirty(_ParentSO);
                AssetDatabase.SaveAssets();

                _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                _InspectorValueChangeCallback?.Invoke(true);
            };

            return list;
        }

        #endregion InternalMethods
    }
}