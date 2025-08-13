using ItemSystem.SubModules;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Tab for managing item containers, such as inventory slots and loot pools.
    /// </summary>
    public class ItemContainerTab : TabBase
    {
        public ItemContainerTab(Action<bool> _InspectorValueChangeCallback, Action<IEnumerable<System.Object>, bool, bool> _TreeviewSelectionChangeCallback, int _BGColor)
        {
            m_Root = new VisualElement();

            m_Root.style.flexDirection = FlexDirection.Column;
            m_Root.style.flexGrow = 1;

            LoadHierarchy(_BGColor);

            SetTabBackgroundColor(_BGColor);

            Add(m_Root);
        }

        protected override void OnSubTabChanged(System.Type _ModuleType, bool _ShowAddAndRemove, bool _LoadSubTypes, bool _ShowInspectorPanel, bool _LoadLocalFiles, int _ButtonColor)
        {
            if (m_FilterPanel != null)
            {
                m_FilterPanel.ClearFilter();
            }

            m_SubTabContent.Clear();
            MethodInfo method = typeof(ItemContainerTab).GetMethod("LoadSubTabHierarchy", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo generic = method.MakeGenericMethod(_ModuleType);
            generic.Invoke(this, new object[] { _ShowAddAndRemove, _ShowInspectorPanel, _LoadSubTypes, _LoadLocalFiles, _ButtonColor });
        }

        protected override void LoadHierarchy(int _ButtonColor)
        {
            m_Root?.Clear();
            m_SubTabMenu?.Clear();
            m_SubTabContent?.Clear();

            m_SubTabMenu = new TabbedMenu(new KeyValuePair<string, System.Type>[]
            {
           new KeyValuePair<string, System.Type>("ItemSlots", typeof(SO_ItemSlot)),
           new KeyValuePair<string, System.Type>("ItemPools", null),
            }, OnSubTabChanged, true, true, true, false, _MainTabColor: _ButtonColor);

            m_SubTabContent = new VisualElement();
            m_SubTabInspectorPanel = new InspectorPanel();

            m_SubTabContent.style.flexDirection = FlexDirection.Row;
            m_SubTabContent.style.flexGrow = 1;

            m_Root.Add(m_SubTabMenu);

            LoadFilterPanel(typeof(SO_ItemSlot), _ButtonColor);
            LoadSubTabHierarchy<SO_ItemSlot>(true, true, true, false, _ButtonColor);
            OnSubTabChanged(typeof(SO_ItemSlot), true, true, true, false, _ButtonColor);

            m_Root.Add(m_SubTabContent);
        }
    }
}