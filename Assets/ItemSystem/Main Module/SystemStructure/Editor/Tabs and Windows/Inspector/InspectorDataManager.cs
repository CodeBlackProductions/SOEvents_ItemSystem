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
        private static List<System.Type> m_BasicDataTypes = new List<System.Type>()
        {
            typeof(string), typeof(int), typeof(float), typeof(bool), typeof(Vector2), typeof(Vector3), typeof(Color)
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
                    else if (m_BasicDataTypes.Contains(_Property.PropertyType) && _Property.Name != "TypeIndex")
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

                dropdownField.style.minHeight = 20;

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

                dropdownField.style.minHeight = 20;

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

            if (_Property.PropertyType == typeof(Dictionary<string, SO_Stat_Base>))
            {
                List<SO_Stat_Base> allStats = ItemEditor_AssetLoader.LoadAssetsByType<SO_Stat_Base>();
                List<string> allTargetUserStats = new List<string>();

                allStats.ForEach(so =>
                {
                    if (so.GetType().IsSubclassOf(typeof(SO_Stat_Base)) && !allTargetUserStats.Contains(so.TargetUserStat))
                    {
                        allTargetUserStats.Add(so.TargetUserStat);
                    }
                });

                Dictionary<string, SO_Stat_Base> statDictionary = dict as Dictionary<string, SO_Stat_Base>;
                if (statDictionary == null)
                {
                    statDictionary = new Dictionary<string, SO_Stat_Base>();
                }

                List<string> selectedStats = new List<string>();
                foreach (KeyValuePair<string, SO_Stat_Base> entry in statDictionary)
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

                    ScrollView scrollview = new ScrollView();
                    selectedList.Add(scrollview);

                    foreach (var stat in selectedStats)
                    {
                        var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                        row.Add(new Label(stat));

                        List<SO_Stat_StaticValue> matchingStaticStats = new List<SO_Stat_StaticValue>();
                        List<SO_Stat> matchingDynamicStats = new List<SO_Stat>();

                        foreach (var s in allStats)
                        {
                            if (s.TargetUserStat == stat)
                            {
                                if (s is SO_Stat_StaticValue)
                                {
                                    matchingStaticStats.Add(s as SO_Stat_StaticValue);
                                }
                                else if (s is SO_Stat)
                                {
                                    matchingDynamicStats.Add(s as SO_Stat);
                                }
                                else
                                {
                                    Debug.LogWarning($"Stat {s.ModuleName} ({s.GetType().Name}) does not match expected types for TargetUserStat: {stat}");
                                }
                            }
                        }

                        List<string> soNames = new List<string>();

                        foreach (var staticStat in matchingStaticStats)
                        {
                            soNames.Add($"{staticStat.ModuleName} ({staticStat.GetType().Name})");
                        }

                        foreach (var dynStat in matchingDynamicStats)
                        {
                            int count = dynStat.GetStatCount();

                            if (count <= 0)
                            {
                                soNames.Add($"No Values Found for {dynStat.ModuleName} ({dynStat.GetType().Name})");
                            }

                            for (int i = 0; i < count; i++)
                            {
                                soNames.Add($"{dynStat.ModuleName}/{dynStat.GetStatValue(i)} ({dynStat.GetType().Name})");
                            }
                        }

                        if (soNames == null || soNames.Count == 0)
                        {
                            selectedStats.Remove(stat);
                            return;
                        }

                        SO_Stat_Base current = statDictionary.FirstOrDefault(x => x.Value.TargetUserStat == stat).Value;

                        string currentName = string.Empty;
                        if (current != null)
                        {
                            if (current is SO_Stat_StaticValue staticVal)
                            {
                                currentName = $"{staticVal.ModuleName} ({staticVal.GetType().Name})";
                            }
                            else if (current is SO_Stat val)
                            {
                                if (val.GetStatCount() > 0)
                                {
                                    currentName = $"{val.ModuleName}/{val.GetStatValue(0)} ({val.GetType().Name})";
                                }
                                else
                                {
                                    currentName = $"No Values Found for {val.ModuleName} ({val.GetType().Name})";
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"Stat {current.ModuleName} ({current.GetType().Name}) does not match expected types for TargetUserStat: {stat}");
                            }
                        }

                        currentName = currentName != string.Empty && currentName != null ? currentName : soNames.FirstOrDefault();

                        DropdownField statDropdown = new DropdownField("", soNames, currentName);
                        statDropdown.RegisterValueChangedCallback(c =>
                        {
                            SO_Stat_Base selectedSO = matchingStaticStats.FirstOrDefault(s => $"{s.ModuleName} ({s.GetType().Name})" == c.newValue);
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

                        scrollview.style.maxHeight = 300;
                        scrollview.Add(row);
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

                        SO_Stat_Base newItem = allStats.FirstOrDefault(so => so.TargetUserStat == selected);
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

                VisualElement selectionContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                selectionContainer.Add(dropdown);
                selectionContainer.Add(addButton);
                selectionContainer.style.minHeight = 20;

                container.Add(selectionContainer);
                container.Add(selectedList);

                updateUI();

                container.style.minHeight = 20;

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

            if (m_BasicDataTypes.Contains(_Property.PropertyType.GetElementType()))
            {
                System.Type elementType = _Property.PropertyType.GetElementType();
                VisualElement basicArrayUI = CreateUIforDataTypeArray(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, elementType);
                _UIParent.Add(basicArrayUI);
                return _UIParent;
            }

            if (_Property.PropertyType == typeof(GameObject[]))
            {
                System.Type elementType = _Property.PropertyType.GetElementType();
                VisualElement projectileArrayUI = CreateUIforDataTypeArray(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, elementType);
                _UIParent.Add(projectileArrayUI);
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

                dropdownField.style.minHeight = 20;

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
            _UIParent.tooltip = _Property.GetAttribute<TooltipAttribute>()?.tooltip;
            System.Type type = _Property.PropertyType;

            VisualElement valueField = CreateFieldForType(
                type,
                _Property.GetValue(_ParentSO),
                newValue =>
                {
                    _Property.SetValue(_ParentSO, newValue);
                    EditorUtility.SetDirty(_ParentSO);
                    AssetDatabase.SaveAssets();
                    _InspectorValueChangeCallback?.Invoke(true);
                },
                () =>
                {
                    _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                });

            if (valueField != null)
            {
                _UIParent.Add(valueField);
            }

            return _UIParent;
        }

        private static VisualElement CreateUIforDataTypeArray(
            ScriptableObject _ParentSO,
            PropertyInfo _Property,
            InspectorPanel _ParentPanel,
            Action<bool> _InspectorValueChangeCallback,
            System.Type _ElementType)
        {
            Array currentArray = _Property.GetValue(_ParentSO) as Array ?? Array.CreateInstance(_ElementType, 0);
            VisualElement listRoot = new VisualElement { style = { flexDirection = FlexDirection.Column, marginBottom = 10 } };

            if (_ElementType == typeof(bool))
            {
                for (int i = 0; i < currentArray.Length; i++)
                {
                    bool value = (bool)currentArray.GetValue(i);
                    Label label = new Label($"Value {i}: {value}")
                    {
                        style =
                        {
                            unityFontStyleAndWeight = FontStyle.Bold,
                            marginBottom = 2,
                            marginLeft = 4
                        }
                    };
                    listRoot.Add(label);
                }
                return listRoot;
            }

            Button addButton = new Button(() =>
            {
                int oldLength = currentArray.Length;
                Array newArray = Array.CreateInstance(_ElementType, oldLength + 1);
                currentArray.CopyTo(newArray, 0);
                object defaultValue = _ElementType.IsValueType ? Activator.CreateInstance(_ElementType) : null;
                newArray.SetValue(defaultValue, oldLength);

                _Property.SetValue(_ParentSO, newArray);
                EditorUtility.SetDirty(_ParentSO);
                AssetDatabase.SaveAssets();
                _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
            })
            { text = $"Add" };

            listRoot.Add(addButton);

            ScrollView scrollView = new ScrollView
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    maxHeight = 300
                }
            };

            for (int i = 0; i < currentArray.Length; i++)
            {
                int index = i;
                VisualElement row = new VisualElement
                {
                    style =
                    {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginBottom = 2
                    }
                };

                object currentValue = currentArray.GetValue(index);
                VisualElement field = CreateFieldForType(
                    _ElementType,
                    currentValue,
                    newValue =>
                    {
                        currentArray.SetValue(newValue, index);
                        _Property.SetValue(_ParentSO, currentArray);
                        EditorUtility.SetDirty(_ParentSO);
                        AssetDatabase.SaveAssets();
                        _InspectorValueChangeCallback?.Invoke(true);
                    },
                    () =>
                    {
                        _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                    });

                Button removeButton = new Button(() =>
                {
                    List<object> tempList = new List<object>(currentArray.Length);
                    for (int j = 0; j < currentArray.Length; j++)
                        if (j != index) tempList.Add(currentArray.GetValue(j));

                    Array newArray = Array.CreateInstance(_ElementType, tempList.Count);
                    for (int k = 0; k < tempList.Count; k++)
                        newArray.SetValue(tempList[k], k);

                    _Property.SetValue(_ParentSO, newArray);
                    EditorUtility.SetDirty(_ParentSO);
                    AssetDatabase.SaveAssets();
                    _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                })
                {
                    text = "Remove",
                    style = { width = 60 }
                };

                row.Add(field);
                row.Add(removeButton);
                scrollView.Add(row);
            }

            listRoot.Add(scrollView);

            return listRoot;
        }

        private static VisualElement CreateFieldForType(System.Type _Type, object _Value, Action<object> _OnChange, Action _OnFocusOut)
        {
            if (_Type == typeof(string))
            {
                TextField field = new TextField
                {
                    value = (string)_Value,
                    style =
                    {
                        flexGrow = 0,
                        flexShrink = 1,
                        width = StyleKeyword.Auto,
                        minWidth = 40,
                        minHeight = 20,
                        marginRight = 6
                    }
                };

                var textInput = field.Q("unity-text-input");
                if (textInput != null)
                {
                    textInput.style.flexGrow = 0;
                    textInput.style.flexShrink = 1;
                    textInput.style.width = StyleKeyword.Auto;
                    textInput.style.minWidth = 40;
                }

                field.RegisterCallback<FocusOutEvent>(evt =>
                {
                    _OnChange(field.value);
                    _OnFocusOut?.Invoke();
                });
                return field;
            }
            if (_Type == typeof(int))
            {
                IntegerField field = new IntegerField
                {
                    value = (int)_Value,
                    style =
                    {
                        flexGrow = 0,
                        flexShrink = 1,
                        width = StyleKeyword.Auto,
                        minWidth = 40,
                        minHeight = 20,
                        marginRight = 6
                    }
                };

                var textInput = field.Q("unity-text-input");
                if (textInput != null)
                {
                    textInput.style.flexGrow = 0;
                    textInput.style.flexShrink = 1;
                    textInput.style.width = StyleKeyword.Auto;
                    textInput.style.minWidth = 40;
                }

                field.RegisterCallback<FocusOutEvent>(evt =>
                {
                    _OnChange(field.value);
                    _OnFocusOut?.Invoke();
                });
                return field;
            }
            if (_Type == typeof(float))
            {
                FloatField field = new FloatField
                {
                    value = (float)_Value,
                    style =
                    {
                        flexGrow = 0,
                        flexShrink = 1,
                        width = StyleKeyword.Auto,
                        minWidth = 40,
                        minHeight = 20,
                        marginRight = 6
                    }
                };

                var textInput = field.Q("unity-text-input");
                if (textInput != null)
                {
                    textInput.style.flexGrow = 0;
                    textInput.style.flexShrink = 1;
                    textInput.style.width = StyleKeyword.Auto;
                    textInput.style.minWidth = 40;
                }

                field.RegisterCallback<FocusOutEvent>(evt =>
                {
                    _OnChange(field.value);
                    _OnFocusOut?.Invoke();
                });
                return field;
            }
            if (_Type == typeof(bool))
            {
                Toggle toggle = new Toggle { value = (bool)_Value };
                toggle.RegisterValueChangedCallback(evt => _OnChange(evt.newValue));
                toggle.style.minHeight = 20;
                return toggle;
            }
            if (_Type == typeof(Vector2))
            {
                VisualElement vectorRow = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Column,
                        alignItems = Align.FlexStart,
                        flexGrow = 0,
                        flexShrink = 1,
                        width = StyleKeyword.Auto,
                        minWidth = 50,
                        borderBottomWidth = 2,
                        borderBottomColor = new Color(0.8f, 0.8f, 0.8f, 1)
                    }
                };
                FloatField Xfield = new FloatField("X")
                {
                    value = ((Vector2)_Value).x,
                    style =
                    {
                        flexGrow = 0,
                        flexShrink = 1,
                        width = StyleKeyword.Auto,
                        minWidth = 40,
                        marginRight = 6
                    }
                };
                VisualElement ve = Xfield;
                ve.ElementAt(0).style.minWidth = 10;

                FloatField Yfield = new FloatField("Y")
                {
                    value = ((Vector2)_Value).y,
                    style =
                    {
                        flexGrow = 0,
                        flexShrink = 1,
                        width = StyleKeyword.Auto,
                        minWidth = 40,
                        marginRight = 6
                    }
                };
                ve = Yfield;
                ve.ElementAt(0).style.minWidth = 10;

                Xfield.RegisterCallback<FocusOutEvent>(evt => _OnChange(new Vector2(Xfield.value, Yfield.value)));

                Yfield.RegisterCallback<FocusOutEvent>(evt => _OnChange(new Vector2(Xfield.value, Yfield.value)));

                vectorRow.Add(Xfield);
                vectorRow.Add(Yfield);

                return vectorRow;
            }
            if (_Type == typeof(Vector3))
            {
                VisualElement vectorRow = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Column,
                        alignItems = Align.FlexStart,
                        flexGrow = 0,
                        flexShrink = 1,
                        width = StyleKeyword.Auto,
                        minWidth = 50,
                        borderBottomWidth = 2,
                        borderBottomColor = new Color(0.8f, 0.8f, 0.8f, 1)
                    }
                };
                FloatField Xfield = new FloatField("X")
                {
                    value = ((Vector3)_Value).x,
                    style =
                    {
                        flexGrow = 0,
                        flexShrink = 1,
                        width = StyleKeyword.Auto,
                        minWidth = 40,
                        marginRight = 6
                    }
                };
                VisualElement ve = Xfield;
                ve.ElementAt(0).style.minWidth = 10;

                FloatField Yfield = new FloatField("Y")
                {
                    value = ((Vector3)_Value).y,
                    style =
                    {
                        flexGrow = 0,
                        flexShrink = 1,
                        width = StyleKeyword.Auto,
                        minWidth = 40,
                        marginRight = 6
                    }
                };
                ve = Yfield;
                ve.ElementAt(0).style.minWidth = 10;

                FloatField Zfield = new FloatField("Z")
                {
                    value = ((Vector3)_Value).z,
                    style =
                    {
                        flexGrow = 0,
                        flexShrink = 1,
                        width = StyleKeyword.Auto,
                        minWidth = 40,
                        marginRight = 6
                    }
                };
                ve = Zfield;
                ve.ElementAt(0).style.minWidth = 10;

                Xfield.RegisterCallback<FocusOutEvent>(evt => _OnChange(new Vector3(Xfield.value, Yfield.value, Zfield.value)));

                Yfield.RegisterCallback<FocusOutEvent>(evt => _OnChange(new Vector3(Xfield.value, Yfield.value, Zfield.value)));

                Zfield.RegisterCallback<FocusOutEvent>(evt => _OnChange(new Vector3(Xfield.value, Yfield.value, Zfield.value)));

                vectorRow.Add(Xfield);
                vectorRow.Add(Yfield);
                vectorRow.Add(Zfield);

                return vectorRow;
            }
            if (_Type == typeof(Color))
            {
                ColorField field = new ColorField { value = (Color)_Value, style = { width = 140, marginRight = 6, minHeight = 20 } };
                field.RegisterValueChangedCallback(evt => _OnChange(evt.newValue));
                return field;
            }
            if (_Type == typeof(GameObject))
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
                    string currentEntry;
                    if (_Value == null)
                    {
                        currentEntry = "Select Projectile";
                        projectileNames.Insert(0, currentEntry);
                    }
                    else
                    {
                        currentEntry = (_Value as GameObject).GetComponent<IProjectile>()?.ProjectileName;
                    }

                    DropdownField dropdownField = new DropdownField(projectileNames, currentEntry);

                    dropdownField.RegisterValueChangedCallback(evt =>
                    _OnChange(
                        projectileList.Find(obj => obj.GetComponent<IProjectile>() != null && obj.GetComponent<IProjectile>()?.ProjectileName == evt.newValue)
                    ));
                    dropdownField.style.minHeight = 20;
                    return dropdownField;
                }
                else
                {
                    Label label = new Label("No projectiles available");
                    label.style.minHeight = 20;
                    return label;
                }
            }
            Debug.LogWarning($"Unsupported field type: {_Type.Name}");
            return null;
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
                dropdownField.style.minHeight = 20;

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
                noTypes.style.minHeight = 20;

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