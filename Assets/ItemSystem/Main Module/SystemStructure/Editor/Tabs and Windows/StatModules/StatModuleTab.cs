using ItemSystem.SubModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Tab for displaying and managing stat modules in the Item System editor.
    /// </summary>
    public class StatModuleTab : TabBase
    {
        public StatModuleTab(Action<bool> _InspectorValueChangeCallback, Action<IEnumerable<System.Object>, bool, bool> _TreeviewSelectionChangeCallback)
        {
            m_InspectorValueChangeCallback = _InspectorValueChangeCallback;
            m_TreeviewSelectionChangeCallback = _TreeviewSelectionChangeCallback;

            m_Root = new VisualElement();

            m_Root.style.flexDirection = FlexDirection.Column;
            m_Root.style.flexGrow = 1;

            LoadHierarchy();

            Add(m_Root);
        }

        protected override void LoadHierarchy()
        {
            m_Root?.Clear();
            m_SubTabMenu?.Clear();
            m_SubTabContent?.Clear();

            List<System.Type> statTypes = ItemEditor_AssetLoader.LoadDerivedTypes(typeof(SO_Stat)).ToList();
            statTypes.AddRange(ItemEditor_AssetLoader.LoadDerivedTypes(typeof(SO_Stat_StaticValue)).ToList());
            List<KeyValuePair<string, System.Type>> statTypePairs = new List<KeyValuePair<string, System.Type>>();

            foreach (System.Type type in statTypes)
            {
                string name = type.Name.Replace("SO_Stat_", "");
                statTypePairs.Add(new KeyValuePair<string, System.Type>(name, type));
            }

            m_SubTabMenu = new TabbedMenu(statTypePairs.ToArray(), OnSubTabChanged, true, true, true, false);

            m_SubTabContent = new VisualElement();
            m_SubTabInspectorPanel = new InspectorPanel();

            m_SubTabContent.style.flexDirection = FlexDirection.Row;
            m_SubTabContent.style.flexGrow = 1;

            m_Root.Add(m_SubTabMenu);

            LoadFilterPanel(typeof(SO_Stat));
            LoadSubTabHierarchy<SO_Stat>(true, true, true, false);
            OnSubTabChanged(typeof(SO_Stat_Integer), true, true, true, false);

            m_Root.Add(m_SubTabContent);
        }

        protected override void OnSubTabChanged(System.Type _ModuleType, bool _ShowAddAndRemove, bool _LoadSubTypes, bool _ShowInspectorPanel, bool _LoadLocalFiles)
        {
            if (m_FilterPanel != null)
            {
                m_FilterPanel.ClearFilter();
            }

            m_SubTabContent.Clear();

            MethodInfo method = typeof(StatModuleTab).GetMethod("LoadSubTabHierarchy", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo generic = method.MakeGenericMethod(_ModuleType);
            generic.Invoke(this, new object[] { _ShowAddAndRemove, _ShowInspectorPanel, _LoadSubTypes, _LoadLocalFiles });
        }
    }
}