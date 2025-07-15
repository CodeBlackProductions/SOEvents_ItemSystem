using Codice.CM.Common;
using ItemSystem.MainModule;
using ItemSystem.SubModules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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

        public FilterPanel(System.Type _ObjectType)
        {
            m_Root = new VisualElement();
            m_FilterPanelContent = new VisualElement();

            m_ObjectType = _ObjectType;

            containsAll = obj => Filter_ModuleChecker.ContainsAllObjects(obj, m_ModulesToCheck);
            containsAny = obj => Filter_ModuleChecker.ContainsAnyObjects(obj, m_ModulesToCheck);

            m_Root.style.flexDirection = FlexDirection.Column;
            m_Root.style.flexGrow = 1;
            m_Root.style.paddingBottom = 10;
            m_Root.style.paddingTop = 10;

            var topDivider = new VisualElement();
            topDivider.style.height = 2;
            topDivider.style.backgroundColor = new StyleColor(Color.grey);
            topDivider.style.marginBottom = 5;

            var bottomDivider = new VisualElement();
            bottomDivider.style.height = 2;
            bottomDivider.style.backgroundColor = new StyleColor(Color.grey);
            bottomDivider.style.marginTop = 5;

            m_Root.Add(topDivider);

            LoadFilterOptions();
            LoadFilterTypes();
            LoadFilterTags();
            LoadSortingOptions(_ObjectType);

            m_FilterPanelContent.style.flexDirection = FlexDirection.Row;
            m_FilterPanelContent.style.flexGrow = 1;

            m_Root.Add(m_FilterPanelContent);

            m_Root.Add(bottomDivider);

            Add(m_Root);
        }

        private void LoadFilterOptions()
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

            m_FilterPanelContent.Add(filterTypeDropdown);
            OnFilterOptionsChanged(containsAll);
        }

        private void LoadFilterTypes()
        {
            List<ScriptableObject> empty = new List<ScriptableObject>();
            InspectorList<ScriptableObject> filterTypeList = new InspectorList<ScriptableObject>(empty, new List<System.Type>() { typeof(SO_Tag) }, "Filter types", true);

            filterTypeList.ItemAddCallback += (item) =>
            {
                OnFilterObjectsChanged(item, true);
            };
            filterTypeList.ItemRemoveCallback += (item) =>
            {
                OnFilterObjectsChanged(item, false);
            };

            m_FilterPanelContent.Add(filterTypeList);
        }

        private void LoadFilterTags()
        {
            List<SO_Tag> empty = new List<SO_Tag>();
            InspectorList<SO_Tag> filterTagList = new InspectorList<SO_Tag>(empty, null, "Filter tags", true);

            filterTagList.ItemAddCallback += (item) =>
            {
                OnFilterObjectsChanged(item, true);
            };
            filterTagList.ItemRemoveCallback += (item) =>
            {
                OnFilterObjectsChanged(item, false);
            };

            m_FilterPanelContent.Add(filterTagList);
        }

        private void LoadSortingOptions(System.Type _Type)
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
            LoadFilterOptions();
            LoadFilterTypes();
            LoadFilterTags();
            LoadSortingOptions(m_ObjectType);
        }

        public void ChangeSortModeType(System.Type _Type) 
        {
            m_FilterPanelContent.Remove(m_SortingTypeDropdown);
            LoadSortingOptions(_Type);
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