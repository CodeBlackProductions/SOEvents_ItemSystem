using ItemSystem.SubModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Tab for displaying and managing stat modules in the Item System editor.
    /// </summary>
    public class StatModuleTab : TabBase
    {
        public StatModuleTab(Action<bool> _InspectorValueChangeCallback, Action<IEnumerable<System.Object>, bool, bool> _TreeviewSelectionChangeCallback, int _BGColor)
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

            List<System.Type> statTypes = ItemEditor_AssetLoader.LoadDerivedTypes(typeof(SO_Stat_DynamicValue)).ToList();
            statTypes.AddRange(ItemEditor_AssetLoader.LoadDerivedTypes(typeof(SO_Stat_StaticValue)).ToList());
            List<KeyValuePair<string, System.Type>> statTypePairs = new List<KeyValuePair<string, System.Type>>();

            foreach (System.Type type in statTypes)
            {
                string name = type.Name.Replace("SO_Stat_", "");
                statTypePairs.Add(new KeyValuePair<string, System.Type>(name, type));
            }

            m_SubTabMenu = new TabbedMenu(statTypePairs.ToArray(), OnSubTabChanged, true, true, true, false, _MainTabColor: _ButtonColor);

            m_SubTabContent = new VisualElement();
            m_SubTabInspectorPanel = new InspectorPanel();

            m_SubTabContent.style.flexDirection = FlexDirection.Row;
            m_SubTabContent.style.flexGrow = 1;

            m_Root.Add(m_SubTabMenu);

            LoadFilterPanel(typeof(SO_Stat_DynamicValue), _ButtonColor);
            LoadSubTabHierarchy<SO_Stat_DynamicValue>(true, true, true, false, _ButtonColor);
            OnSubTabChanged(typeof(SO_Stat_Integer), true, true, true, false, _ButtonColor);

            m_Root.Add(m_SubTabContent);
        }

        protected override void OnSubTabChanged(System.Type _ModuleType, bool _ShowAddAndRemove, bool _LoadSubTypes, bool _ShowInspectorPanel, bool _LoadLocalFiles, int _ButtonColor)
        {
            if (m_FilterPanel != null)
            {
                m_FilterPanel.ClearFilter();
            }

            m_SubTabContent.Clear();

            MethodInfo method = typeof(StatModuleTab).GetMethod("LoadSubTabHierarchy", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo generic = method.MakeGenericMethod(_ModuleType);
            generic.Invoke(this, new object[] { _ShowAddAndRemove, _ShowInspectorPanel, _LoadSubTypes, _LoadLocalFiles , _ButtonColor });
        }
    }
}