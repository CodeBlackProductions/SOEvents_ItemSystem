using ItemSystem.MainModule;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Tab for managing and editing item modules in the Item System editor.
    /// </summary>
    public class ItemModuleTab : TabBase
    {
        public ItemModuleTab(Action<bool> _InspectorValueChangeCallback, Action<IEnumerable<System.Object>, bool, bool> _TreeviewSelectionChangeCallback, int _BGColor)
        {
            m_InspectorValueChangeCallback = _InspectorValueChangeCallback;
            m_TreeviewSelectionChangeCallback = _TreeviewSelectionChangeCallback;

            m_Root = new VisualElement();

            m_Root.style.flexDirection = FlexDirection.Column;
            m_Root.style.flexGrow = 1;

            LoadHierarchy(_BGColor);

            SetTabBackgroundColor(_BGColor);

            Add(m_Root);
        }

        protected override void LoadHierarchy(int _ButtonColor)
        {
            m_Root?.Clear();
            m_SubTabMenu?.Clear();
            m_SubTabContent?.Clear();

            m_SubTabMenu = new TabbedMenu(new KeyValuePair<string, System.Type>[]
            {
           new KeyValuePair<string, System.Type>("Items", typeof(SO_Item)),
           new KeyValuePair<string, System.Type>("Classes", typeof(SO_Item_Class)),
           new KeyValuePair<string, System.Type>("Types", typeof(SO_Class_Type)),
           new KeyValuePair<string, System.Type>("Effects", typeof(SO_Item_Effect)),
           new KeyValuePair<string, System.Type>("Triggers", typeof(SO_Effect_Trigger))
            }, OnSubTabChanged, true, true, true, false, _MainTabColor: _ButtonColor);

            m_SubTabContent = new VisualElement();
            m_SubTabInspectorPanel = new InspectorPanel(_ButtonColor);

            m_SubTabContent.style.flexDirection = FlexDirection.Row;
            m_SubTabContent.style.flexGrow = 1;

            m_Root.Add(m_SubTabMenu);

            LoadFilterPanel(typeof(SO_Item), _ButtonColor);
            LoadSubTabHierarchy<SO_Item>(true, true, true, false, _ButtonColor);
            OnSubTabChanged(typeof(SO_Item), true, true, true, false, _ButtonColor);

            m_Root.Add(m_SubTabContent);
        }

        protected override void OnSubTabChanged(System.Type _ModuleType, bool _ShowAddAndRemove, bool _LoadSubTypes, bool _ShowInspectorPanel, bool _LoadLocalFiles, int _MainTabColor)
        {
            if (m_FilterPanel != null)
            {
                m_FilterPanel.ClearFilter();
            }

            m_SubTabContent.Clear();

            MethodInfo method = typeof(ItemModuleTab).GetMethod("LoadSubTabHierarchy", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo generic = method.MakeGenericMethod(_ModuleType);
            generic.Invoke(this, new object[] { _ShowAddAndRemove, _ShowInspectorPanel, _LoadSubTypes, _LoadLocalFiles, _MainTabColor });
        }
    }
}