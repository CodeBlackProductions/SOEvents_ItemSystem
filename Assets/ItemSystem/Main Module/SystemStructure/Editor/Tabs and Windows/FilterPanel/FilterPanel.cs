using ItemSystem.MainModule;
using ItemSystem.SubModules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// This class is used to create a filter panel for the Item System.
    /// It allows the user to filter item modules based on their types and properties.
    /// </summary>
    /// <remarks>
    /// The FilterPanel class is a VisualElement that contains a dropdown for selecting filter options.
    /// It uses the Filter_TypeChecker class to check if an object contains a specific type or any of the types in a list.
    /// </remarks>
    public class FilterPanel : VisualElement
    {
        private VisualElement m_Root;
        private VisualElement m_FilterPanelContent;

        private List<ScriptableObject> m_ModulesToCheck = new List<ScriptableObject>();
        private Func<object, bool> m_Filter = (o) => true;

        private Func<object, bool> containsAll;
        private Func<object, bool> containsAny;

        private TreeViewSortMode m_SortMode = TreeViewSortMode.None;
        private System.Type m_ObjectType;
        private DropdownField m_SortingTypeDropdown;

        private int m_ButtonColor = -1;

        private static readonly Dictionary<System.Type, List<TreeViewSortMode>> m_SortOptionsPerType = new()
        {
            { typeof(SO_Item), new List<TreeViewSortMode> { TreeViewSortMode.None, TreeViewSortMode.Alphabetical, TreeViewSortMode.ItemClass, TreeViewSortMode.ClassType, TreeViewSortMode.Rarity } },
            { typeof(SO_Item_Effect), new List<TreeViewSortMode> { TreeViewSortMode.None, TreeViewSortMode.Alphabetical, TreeViewSortMode.AllowedTargets, TreeViewSortMode.Target, TreeViewSortMode.Trigger } }
        };

        private static readonly List<TreeViewSortMode> m_DefaultSortOptions = new List<TreeViewSortMode>
        {
            TreeViewSortMode.None,
            TreeViewSortMode.Alphabetical
        };

        public Action<Func<object, bool>, List<ScriptableObject>> OnFilterChangedCallback;
        public Action<TreeViewSortMode> OnSortModeChangedCallback;

        public FilterPanel(System.Type _ObjectType, int _ButtonColor)
        {
            m_Root = new VisualElement();
            m_FilterPanelContent = new VisualElement();

            m_ObjectType = _ObjectType;

            containsAll = obj => Filter_ModuleChecker.ContainsAllObjects(obj, m_ModulesToCheck);
            containsAny = obj => Filter_ModuleChecker.ContainsAnyObjects(obj, m_ModulesToCheck);

            m_Root.style.flexDirection = FlexDirection.Column;
            m_Root.style.flexGrow = 1;
            m_Root.style.paddingBottom = 10;
            m_Root.style.paddingTop = 5;

            m_Root.style.backgroundColor = Color.clear;

            m_Root.style.borderBottomWidth = 2;
            m_Root.style.borderBottomColor = new StyleColor(Color.grey);
            m_Root.style.paddingBottom = 5;
            m_Root.style.marginBottom = 5;

            m_Root.style.borderTopWidth = 2;
            m_Root.style.borderTopColor = new StyleColor(Color.grey);
            m_Root.style.paddingTop = 5;
            m_Root.style.marginTop = 5;

            m_ButtonColor = _ButtonColor;

            LoadFilterOptions(_ButtonColor);
            LoadFilterTypes(_ButtonColor);
            LoadFilterTags(_ButtonColor);
            LoadSortingOptions(_ObjectType, _ButtonColor);

            m_FilterPanelContent.style.flexDirection = FlexDirection.Row;
            m_FilterPanelContent.style.flexGrow = 1;

            m_Root.Add(m_FilterPanelContent);

            Add(m_Root);
        }

        private void LoadFilterOptions(int _ButtonColor)
        {
            var filterTypeDropdown = new DropdownField("", new List<string> { "Contains All", "Contains Any", "Contains None" }, 0);

            filterTypeDropdown.RegisterValueChangedCallback(evt =>
            {
                switch (evt.newValue)
                {
                    case "Contains All":
                        OnFilterOptionsChanged(containsAll);
                        break;

                    case "Contains Any":
                        OnFilterOptionsChanged(containsAny);
                        break;

                    case "Contains None":
                        OnFilterOptionsChanged(obj => !containsAny(obj));
                        break;

                    default:
                        Debug.LogWarning($"Unknown filter option: {evt.newValue}");
                        OnFilterOptionsChanged(containsAll);
                        break;
                }
            });

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ItemSystem/Main Module/SystemStructure/Editor/Tabs and Windows/UI_Styles.uss");
            filterTypeDropdown.styleSheets.Add(styleSheet);

            VisualElement ve = filterTypeDropdown;
            ve.ElementAt(0).AddToClassList($"tab-c-{_ButtonColor}");

            filterTypeDropdown.style.paddingLeft = 10;

            m_FilterPanelContent.Add(filterTypeDropdown);
            OnFilterOptionsChanged(containsAll);
        }

        private void LoadFilterTypes(int _ButtonColor)
        {
            List<ScriptableObject> empty = new List<ScriptableObject>();
            InspectorList<ScriptableObject> filterTypeList = new InspectorList<ScriptableObject>(empty, new List<System.Type>() { typeof(SO_Tag) }, "Filter types", true, _ButtonColor, true);

            filterTypeList.ItemAddCallback += (item) =>
            {
                OnFilterObjectsChanged(item, true);
            };
            filterTypeList.ItemRemoveCallback += (item) =>
            {
                OnFilterObjectsChanged(item, false);
            };

            filterTypeList.style.paddingLeft = 10;

            m_FilterPanelContent.Add(filterTypeList);
        }

        private void LoadFilterTags(int _ButtonColor)
        {
            List<SO_Tag> empty = new List<SO_Tag>();
            InspectorList<SO_Tag> filterTagList = new InspectorList<SO_Tag>(empty, null, "Filter tags", true, _ButtonColor, true);

            filterTagList.ItemAddCallback += (item) =>
            {
                OnFilterObjectsChanged(item, true);
            };
            filterTagList.ItemRemoveCallback += (item) =>
            {
                OnFilterObjectsChanged(item, false);
            };

            filterTagList.style.paddingLeft = 10;

            m_FilterPanelContent.Add(filterTagList);
        }

        private void LoadSortingOptions(System.Type _Type, int _ButtonColor)
        {
            List<TreeViewSortMode> allowedSortModes;
            if (!m_SortOptionsPerType.TryGetValue(_Type, out allowedSortModes))
                allowedSortModes = m_DefaultSortOptions;

            m_SortingTypeDropdown = new DropdownField(
                "Sort by",
                allowedSortModes.Select(m => m.ToString()).ToList(),
                allowedSortModes[0].ToString()
            );
            m_SortingTypeDropdown.RegisterValueChangedCallback(evt =>
            {
                OnSortModeChanged((TreeViewSortMode)Enum.Parse(typeof(TreeViewSortMode), evt.newValue));
            });

            StyleSheet styleSheet = UI_Styles_Lib.GetUIStyles();
            m_SortingTypeDropdown.styleSheets.Add(styleSheet);
            m_SortingTypeDropdown.style.position = Position.Absolute;
            m_SortingTypeDropdown.style.height = new Length(95, LengthUnit.Percent);
            m_SortingTypeDropdown.style.right = new Length(1, LengthUnit.Percent);

            VisualElement ve = m_SortingTypeDropdown;
            ve.ElementAt(0).style.minWidth = 10;
            ve.ElementAt(1).AddToClassList($"tab-c-{_ButtonColor}");

            m_FilterPanelContent.Add(m_SortingTypeDropdown);

            OnSortModeChanged(allowedSortModes[0]);
        }

        private void OnSortModeChanged(TreeViewSortMode _SortMode)
        {
            m_SortMode = _SortMode;
            OnSortModeChangedCallback?.Invoke(_SortMode);
        }

        private void OnFilterOptionsChanged(Func<object, bool> _Filter)
        {
            m_Filter = _Filter;
            OnFilterChangedCallback?.Invoke(m_Filter, m_ModulesToCheck);
        }

        private void OnFilterObjectsChanged(ScriptableObject _Object, bool _Additive)
        {
            if (_Object == null) return;

            if (_Additive && !m_ModulesToCheck.Contains(_Object))
            {
                m_ModulesToCheck.Add(_Object);
            }
            else if (m_ModulesToCheck.Contains(_Object))
            {
                m_ModulesToCheck.Remove(_Object);
            }

            OnFilterChangedCallback?.Invoke(m_Filter, m_ModulesToCheck);
        }

        public void ClearFilter()
        {
            m_FilterPanelContent.Clear();
            LoadFilterOptions(m_ButtonColor);
            LoadFilterTypes(m_ButtonColor);
            LoadFilterTags(m_ButtonColor);
            LoadSortingOptions(m_ObjectType, m_ButtonColor);
        }

        public void ChangeSortModeType(System.Type _Type)
        {
            m_FilterPanelContent.Remove(m_SortingTypeDropdown);
            LoadSortingOptions(_Type, m_ButtonColor);
        }
    }

    public enum TreeViewSortMode
    {
        None,
        Alphabetical,
        ItemClass,
        ClassType,
        Rarity,
        Target,
        AllowedTargets,
        Trigger
    }
}