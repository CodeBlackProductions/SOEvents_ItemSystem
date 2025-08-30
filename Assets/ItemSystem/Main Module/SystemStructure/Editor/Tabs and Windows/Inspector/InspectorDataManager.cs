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
            Action<bool> _InspectorValueChangeCallback,
            int _MainTabColor)
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
                        return CreateUIforEnum(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent, _MainTabColor);
                    }
                    else if (_Property.PropertyType.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        return CreateUIforSO(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent, _MainTabColor);
                    }
                    else if (typeof(IDictionary).IsAssignableFrom(_Property.PropertyType))
                    {
                        return CreateUIforDictionary(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent, _MainTabColor);
                    }
                    else if (typeof(Array).IsAssignableFrom(_Property.PropertyType))
                    {
                        return CreateUIforArray(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent, _MainTabColor);
                    }
                    else if (typeof(GameObject).IsAssignableFrom(_Property.PropertyType))
                    {
                        return CreateUIforProjectiles(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent, _MainTabColor);
                    }
                    else if (m_BasicDataTypes.Contains(_Property.PropertyType) && _Property.Name != "TypeIndex")
                    {
                        return CreateUIforBasicDataTypes(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent);
                    }
                    else if (_Property.Name == "TypeIndex")
                    {
                        return CreateUIforTypeSelection(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent, _MainTabColor);
                    }
                    else if (typeof(SerializableKeyValuePair<SO_Stat, int>).IsAssignableFrom(_Property.PropertyType))
                    {
                        return CreateUIforStat(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent, _MainTabColor);
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

        private static VisualElement CreateUIforStat(
        ScriptableObject _ParentSO,
        PropertyInfo _Property,
        InspectorPanel _ParentPanel,
        Action<bool> _InspectorValueChangeCallback,
        VisualElement _UIParent,
        int _MainTabColor)
        {
            List<SO_Stat> soList = ItemEditor_AssetLoader.LoadAssetsByType<SO_Stat>();
            if (soList == null)
            {
                soList = new List<SO_Stat>();
            }
            else
            {
                soList = AttributeFilterHelper.FilterEntries(_Property, soList);
            }
            List<string> soNames = new List<string>();
            for (int i = 0; i < soList.Count; i++)
            {
                if (soList[i] is SO_Stat_DynamicValue dyn)
                {
                    for (int c = 0; c < dyn.GetStatCount(); c++)
                    {
                        soNames.Add($"{dyn.TargetUserStat}/{dyn.GetStatValue(c).ToString().ToLowerInvariant()} ({dyn.GetType().Name})");
                    }
                }
                else
                {
                    soNames.Add($"{(soList[i] as SO_Stat_StaticValue).TargetUserStat} ({soList[i].GetType().Name})");
                }
            }

            if (soNames.Count > 0)
            {
                SerializableKeyValuePair<SO_Stat, int> statPair = (SerializableKeyValuePair<SO_Stat, int>)_Property.GetValue(_ParentSO);
                string currentEntry = null;

                if (statPair?.Key != null && AttributeFilterHelper.EntryFitsFilters(_Property, statPair.Key))
                {
                    if (statPair.Key is SO_Stat_DynamicValue stat)
                    {
                        currentEntry = $"{stat.TargetUserStat}/{stat.GetStatValue(statPair.Value).ToString().ToLowerInvariant()} ({stat.GetType().Name})";
                    }
                    else
                    {
                        currentEntry = $"{statPair.Key.TargetUserStat} ({statPair.Key.GetType().Name})";
                    }
                }
                else
                {
                    currentEntry = soNames.FirstOrDefault();
                }

                if (currentEntry == null)
                {
                    currentEntry = $"Could not find values for {_Property.Name}";
                    soNames.Insert(0, currentEntry);
                }

                DropdownField dropdownField = new DropdownField(soNames, currentEntry);

                dropdownField.RegisterValueChangedCallback(c =>
                {
                    string selected = c.newValue;

                    SO_Stat selectedStat = null;
                    int selectedIndex = 0;

                    bool found = false;
                    foreach (var stat in soList)
                    {
                        if (stat is SO_Stat_DynamicValue dyn)
                        {
                            for (int i = 0; i < dyn.GetStatCount(); i++)
                            {
                                string label = $"{dyn.TargetUserStat}/{dyn.GetStatValue(i).ToString().ToLowerInvariant()} ({dyn.GetType().Name})";
                                if (label == selected)
                                {
                                    selectedStat = dyn;
                                    selectedIndex = i;
                                    found = true;
                                    break;
                                }
                            }
                        }
                        else if (stat is SO_Stat_StaticValue statValue)
                        {
                            string label = $"{statValue.TargetUserStat} ({statValue.GetType().Name})";
                            if (label == selected)
                            {
                                selectedStat = statValue;
                                selectedIndex = 0;
                                found = true;
                                break;
                            }
                        }
                        if (found) break;
                    }

                    if (selectedStat != null)
                    {
                        var newPair = new SerializableKeyValuePair<SO_Stat, int>(selectedStat, selectedIndex);
                        _Property.SetValue(_ParentSO, newPair);

                        EditorUtility.SetDirty(_ParentSO);
                        AssetDatabase.SaveAssets();
                        _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                        _InspectorValueChangeCallback?.Invoke(true);
                    }
                });

                dropdownField.style.minHeight = 20;

                StyleSheet buttonStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ItemSystem/Main Module/SystemStructure/Editor/Tabs and Windows/TabButton.uss");
                dropdownField.styleSheets.Add(buttonStyle);

                if (_MainTabColor != -1)
                {
                    VisualElement ve = dropdownField;
                    ve.ElementAt(0).AddToClassList($"tab-c-{_MainTabColor}");
                }
                else
                {
                    VisualElement ve = dropdownField;
                    ve.ElementAt(0).AddToClassList("tab-c-default");
                }

                _UIParent.Add(dropdownField);

                return _UIParent;
            }

            Debug.LogWarning($"ScriptableObject entries could not be found for {_ParentSO} : {_Property.Name}");
            return null;
        }

        private static VisualElement CreateUIforEnum(
           ScriptableObject _ParentSO,
           PropertyInfo _Property,
           InspectorPanel _ParentPanel,
           Action<bool> _InspectorValueChangeCallback,
           VisualElement _UIParent,
           int _MainTabColor)
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

                StyleSheet buttonStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ItemSystem/Main Module/SystemStructure/Editor/Tabs and Windows/TabButton.uss");
                dropdownField.styleSheets.Add(buttonStyle);

                if (_MainTabColor != -1)
                {
                    VisualElement ve = dropdownField;
                    ve.ElementAt(0).AddToClassList($"tab-c-{_MainTabColor}");
                }
                else
                {
                    VisualElement ve = dropdownField;
                    ve.ElementAt(0).AddToClassList("tab-c-default");
                }

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
            VisualElement _UIParent,
            int _MainTabColor)
        {
            List<ScriptableObject> soList = ItemEditor_AssetLoader.LoadAssetsByType<ScriptableObject>();
            if (soList == null)
            {
                soList = new List<ScriptableObject>();
            }
            else
            {
                soList = AttributeFilterHelper.FilterEntries(_Property, soList);
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

                if (propertyVal != null && AttributeFilterHelper.EntryFitsFilters(_Property, propertyVal))
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

                StyleSheet buttonStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ItemSystem/Main Module/SystemStructure/Editor/Tabs and Windows/TabButton.uss");
                dropdownField.styleSheets.Add(buttonStyle);

                if (_MainTabColor != -1)
                {
                    VisualElement ve = dropdownField;
                    ve.ElementAt(0).AddToClassList($"tab-c-{_MainTabColor}");
                }
                else
                {
                    VisualElement ve = dropdownField;
                    ve.ElementAt(0).AddToClassList("tab-c-default");
                }

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
            VisualElement _UIParent,
            int _MainTabColor)
        {
            object dict = _Property.GetValue(_ParentSO);

            PropertyInfo indexProp = _ParentSO.GetType().GetProperty("StatIndices");
            object indexDict = indexProp?.GetValue(_ParentSO);

            if (_Property.PropertyType == typeof(Dictionary<string, SO_Stat>))
            {
                List<SO_Stat> allStats = ItemEditor_AssetLoader.LoadAssetsByType<SO_Stat>();
                allStats = AttributeFilterHelper.FilterEntries(_Property, allStats);

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
                Dictionary<string, int> statIndexDictionary = indexDict as Dictionary<string, int>;
                if (statIndexDictionary == null)
                {
                    statIndexDictionary = new Dictionary<string, int>();
                }

                List<string> selectedStats = new List<string>();
                foreach (KeyValuePair<string, SO_Stat> entry in statDictionary)
                {
                    if (!selectedStats.Contains(entry.Value.TargetUserStat) && AttributeFilterHelper.EntryFitsFilters(_Property, entry.Value))
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

                StyleSheet buttonStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ItemSystem/Main Module/SystemStructure/Editor/Tabs and Windows/TabButton.uss");
                dropdown.styleSheets.Add(buttonStyle);

                if (_MainTabColor != -1)
                {
                    VisualElement ve = dropdown;
                    ve.ElementAt(0).AddToClassList($"tab-c-{_MainTabColor}");
                }
                else
                {
                    VisualElement ve = dropdown;
                    ve.ElementAt(0).AddToClassList("tab-c-default");
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
                        List<SO_Stat_DynamicValue> matchingDynamicStats = new List<SO_Stat_DynamicValue>();

                        foreach (var s in allStats)
                        {
                            if (s.TargetUserStat == stat)
                            {
                                if (s is SO_Stat_StaticValue)
                                {
                                    matchingStaticStats.Add(s as SO_Stat_StaticValue);
                                }
                                else if (s is SO_Stat_DynamicValue)
                                {
                                    matchingDynamicStats.Add(s as SO_Stat_DynamicValue);
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
                                soNames.Add($"{dynStat.ModuleName}/{dynStat.GetStatValue(i).ToString().ToLowerInvariant()} ({dynStat.GetType().Name})");
                            }
                        }

                        if (soNames == null || soNames.Count == 0)
                        {
                            selectedStats.Remove(stat);
                            return;
                        }

                        SO_Stat current = statDictionary.FirstOrDefault(x => x.Value.TargetUserStat == stat).Value;

                        string currentName = string.Empty;
                        if (current != null)
                        {
                            if (current is SO_Stat_StaticValue staticVal)
                            {
                                currentName = $"{staticVal.ModuleName} ({staticVal.GetType().Name})";
                            }
                            else if (current is SO_Stat_DynamicValue val)
                            {
                                if (val.GetStatCount() > 0)
                                {
                                    int index = statIndexDictionary.ContainsKey(val.TargetUserStat) ? statIndexDictionary[val.TargetUserStat] : 0;

                                    currentName = $"{val.ModuleName}/{val.GetStatValue(index).ToString().ToLowerInvariant()} ({val.GetType().Name})";
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
                            SO_Stat selectedSO = null;
                            int selectedIndex = 0;

                            selectedSO = matchingStaticStats.FirstOrDefault(s => $"{s.ModuleName} ({s.GetType().Name})" == c.newValue);

                            if (selectedSO == null)
                            {
                                foreach (var dynStat in matchingDynamicStats)
                                {
                                    for (int i = 0; i < dynStat.GetStatCount(); i++)
                                    {
                                        string formatted = $"{dynStat.ModuleName}/{dynStat.GetStatValue(i).ToString().ToLowerInvariant()} ({dynStat.GetType().Name})";
                                        if (formatted.Equals(c.newValue, StringComparison.OrdinalIgnoreCase))
                                        {
                                            selectedSO = dynStat;
                                            selectedIndex = i;
                                            break;
                                        }
                                    }
                                    if (selectedSO != null) break;
                                }
                            }

                            if (selectedSO != null)
                            {
                                string oldKey = statDictionary.FirstOrDefault(x => x.Value.TargetUserStat == stat).Key;
                                if (!string.IsNullOrEmpty(oldKey))
                                {
                                    statIndexDictionary.Remove(oldKey);
                                    statDictionary.Remove(oldKey);
                                }

                                statDictionary[selectedSO.TargetUserStat] = selectedSO;

                                if (selectedSO is SO_Stat_DynamicValue dyn)
                                {
                                    statIndexDictionary[selectedSO.TargetUserStat] = selectedIndex;
                                }
                                else
                                {
                                    statIndexDictionary[selectedSO.TargetUserStat] = 0;
                                }

                                _Property.SetValue(_ParentSO, statDictionary);
                                indexProp.SetValue(_ParentSO, statIndexDictionary);
                                EditorUtility.SetDirty(_ParentSO);
                                AssetDatabase.SaveAssets();
                                _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                                _InspectorValueChangeCallback?.Invoke(true);

                                updateUI();
                            }
                        });

                        StyleSheet buttonStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ItemSystem/Main Module/SystemStructure/Editor/Tabs and Windows/TabButton.uss");
                        statDropdown.styleSheets.Add(buttonStyle);

                        if (_MainTabColor != -1)
                        {
                            VisualElement ve = statDropdown;
                            ve.ElementAt(0).AddToClassList($"tab-c-{_MainTabColor}");
                        }
                        else
                        {
                            VisualElement ve = statDropdown;
                            ve.ElementAt(0).AddToClassList("tab-c-default");
                        }

                        row.Add(statDropdown);

                        var removeButton = new Button(() =>
                        {
                            selectedStats.Remove(stat);
                            dropdown.choices = allTargetUserStats.Except(selectedStats).ToList();
                            dropdown.value = dropdown.choices.FirstOrDefault();
                            updateUI();

                            string toRemove = statDictionary.FirstOrDefault(x => x.Value.TargetUserStat == stat).Key;
                            statDictionary.Remove(toRemove);
                            statIndexDictionary.Remove(toRemove);
                            _Property.SetValue(_ParentSO, statDictionary);
                            indexProp.SetValue(_ParentSO, statIndexDictionary);
                            EditorUtility.SetDirty(_ParentSO);
                            AssetDatabase.SaveAssets();
                            _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                            _InspectorValueChangeCallback?.Invoke(true);
                        })
                        { text = "Remove" };

                        removeButton.styleSheets.Add(buttonStyle);
                        if (_MainTabColor != -1)
                        {
                            removeButton.AddToClassList($"tab-c-{_MainTabColor}");
                        }
                        else
                        {
                            removeButton.AddToClassList("tab-c-default");
                        }

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

                        SO_Stat newItem = allStats.FirstOrDefault(so => so.TargetUserStat == selected);
                        statDictionary.Add(newItem.TargetUserStat, newItem);
                        statIndexDictionary.Add(newItem.TargetUserStat, 0);
                        _Property.SetValue(_ParentSO, statDictionary);
                        indexProp.SetValue(_ParentSO, statIndexDictionary);
                        EditorUtility.SetDirty(_ParentSO);
                        AssetDatabase.SaveAssets();
                        _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                        _InspectorValueChangeCallback?.Invoke(true);
                    }
                })
                { text = "Add" };

                addButton.styleSheets.Add(buttonStyle);

                if (_MainTabColor != -1)
                {
                    addButton.AddToClassList($"tab-c-{_MainTabColor}");
                }
                else
                {
                    addButton.AddToClassList("tab-c-default");
                }

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
            VisualElement _UIParent,
            int _MainTabColor)
        {
            if (_Property.PropertyType == typeof(SO_Item_Effect[]))
            {
                InspectorList<SO_Item_Effect> effectList = ConvertArrayToInspectorList<SO_Item_Effect>(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, "Effects", true, _MainTabColor);

                _UIParent.Add(effectList);
                return _UIParent;
            }

            if (_Property.PropertyType == typeof(SO_Class_Type[]))
            {
                InspectorList<SO_Class_Type> typeList = ConvertArrayToInspectorList<SO_Class_Type>(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, "Types", true, _MainTabColor);

                _UIParent.Add(typeList);
                return _UIParent;
            }

            if (_Property.PropertyType == typeof(SO_Item_Class[]))
            {
                InspectorList<SO_Item_Class> classList = ConvertArrayToInspectorList<SO_Item_Class>(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, "Classes", true, _MainTabColor);

                _UIParent.Add(classList);
                return _UIParent;
            }

            if (_Property.PropertyType == typeof(SO_Tag[]))
            {
                InspectorList<SO_Tag> classList = ConvertArrayToInspectorList<SO_Tag>(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, "Tags", true, _MainTabColor);

                _UIParent.Add(classList);
                return _UIParent;
            }

            if (_Property.PropertyType == typeof(SO_Stat[]))
            {
                InspectorList<SO_Stat> statList = ConvertArrayToInspectorList<SO_Stat>(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, "Stats", true, _MainTabColor);

                _UIParent.Add(statList);
                return _UIParent;
            }

            if (_Property.PropertyType == typeof(SO_ToolTip[]))
            {
                InspectorList<SO_ToolTip> toolTipList = ConvertArrayToInspectorList<SO_ToolTip>(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, "ToolTips", true, _MainTabColor);

                _UIParent.Add(toolTipList);
                return _UIParent;
            }

            if (m_BasicDataTypes.Contains(_Property.PropertyType.GetElementType()))
            {
                System.Type elementType = _Property.PropertyType.GetElementType();
                VisualElement basicArrayUI = CreateUIforDataTypeArray(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, elementType, _MainTabColor);
                _UIParent.Add(basicArrayUI);
                return _UIParent;
            }

            if (_Property.PropertyType == typeof(GameObject[]))
            {
                System.Type elementType = _Property.PropertyType.GetElementType();
                VisualElement projectileArrayUI = CreateUIforDataTypeArray(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, elementType, _MainTabColor);
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
           VisualElement _UIParent,
           int _MainTabColor)
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

                StyleSheet buttonStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ItemSystem/Main Module/SystemStructure/Editor/Tabs and Windows/TabButton.uss");
                dropdownField.styleSheets.Add(buttonStyle);

                if (_MainTabColor != -1)
                {
                    VisualElement ve = dropdownField;
                    ve.ElementAt(0).AddToClassList($"tab-c-{_MainTabColor}");
                }
                else
                {
                    VisualElement ve = dropdownField;
                    ve.ElementAt(0).AddToClassList("tab-c-default");
                }

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

            valueField.style.flexGrow = 1;

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
            System.Type _ElementType,
            int _MainTabColor)
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

            StyleSheet buttonStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ItemSystem/Main Module/SystemStructure/Editor/Tabs and Windows/TabButton.uss");
            addButton.styleSheets.Add(buttonStyle);

            if (_MainTabColor != -1)
            {
                addButton.AddToClassList($"tab-c-{_MainTabColor}");
            }
            else
            {
                addButton.AddToClassList("tab-c-default");
            }

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

                removeButton.styleSheets.Add(buttonStyle);

                if (_MainTabColor != -1)
                {
                    removeButton.AddToClassList($"tab-c-{_MainTabColor}");
                }
                else
                {
                    removeButton.AddToClassList("tab-c-default");
                }

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
           VisualElement _UIParent,
           int _MainTabColor)
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

                StyleSheet buttonStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ItemSystem/Main Module/SystemStructure/Editor/Tabs and Windows/TabButton.uss");
                dropdownField.styleSheets.Add(buttonStyle);

                if (_MainTabColor != -1)
                {
                    VisualElement ve = dropdownField;
                    ve.ElementAt(0).AddToClassList($"tab-c-{_MainTabColor}");
                }
                else
                {
                    VisualElement ve = dropdownField;
                    ve.ElementAt(0).AddToClassList("tab-c-default");
                }

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
            bool _ShowAddAndRemove,
            int _MainTabColor) where T : ScriptableObject
        {
            T[] array = _Property.GetValue(_ParentSO) as T[];
            if (array == null)
            {
                array = new T[0];
            }

            List<T> filteredList = null;
            bool canFilter = false;

            var filters = _Property.GetCustomAttributes(typeof(ItemToolkitFilter), false);
            if (filters != null && filters.Length > 0)
            {
                var hasModuleName = typeof(T).GetProperty("ModuleName") != null;
                var hasTypeFilter = filters.Cast<ItemToolkitFilter>().Any(f => f.Types != null && f.Types.Length > 0);
                if (hasModuleName || hasTypeFilter)
                {
                    canFilter = true;
                }
            }

            if (canFilter)
            {
                filteredList = AttributeFilterHelper.FilterEntries(_Property, array.ToList());
            }
            else
            {
                filteredList = array.ToList();
            }

            InspectorList<T> list = new InspectorList<T>(filteredList, null, _Title, _ShowAddAndRemove, _MainTabColor);

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