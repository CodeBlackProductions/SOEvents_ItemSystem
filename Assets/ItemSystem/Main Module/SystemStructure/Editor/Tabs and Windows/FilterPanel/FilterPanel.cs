using System;
using System.Collections.Generic;
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

        public Action<Func<object, bool>, List<ScriptableObject>> OnFilterChangedCallback;

        public FilterPanel()
        {
            m_Root = new VisualElement();
            m_FilterPanelContent = new VisualElement();

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
            InspectorList<ScriptableObject> filterTypeList = new InspectorList<ScriptableObject>(empty, "Filter types", true);

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

        private void OnFilterOptionsChanged(Func<object, bool> _Filter)
        {
            m_Filter = _Filter;
            OnFilterChangedCallback?.Invoke(m_Filter,m_ModulesToCheck);
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
        }
    }
}