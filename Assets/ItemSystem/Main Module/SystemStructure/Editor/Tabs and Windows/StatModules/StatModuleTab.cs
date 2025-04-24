using ItemSystem.SubModules;
using System;
using System.Collections.Generic;
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

            m_SubTabMenu = new TabbedMenu(new KeyValuePair<string, Type>[]
            {
           new KeyValuePair<string, Type>("Integer", typeof(SO_Stat_Integer)),
           new KeyValuePair<string, Type>("Float", typeof(SO_Stat_Float)),
           new KeyValuePair<string, Type>("String", typeof(SO_Stat_String)),
            }, OnSubTabChanged, true, true, true, false);

            m_SubTabContent = new VisualElement();
            m_SubTabInspectorPanel = new InspectorPanel();

            m_SubTabContent.style.flexDirection = FlexDirection.Row;
            m_SubTabContent.style.flexGrow = 1;

            m_Root.Add(m_SubTabMenu);

            LoadSubTabHierarchy<SO_Stat>(true, true, true, false);
            OnSubTabChanged(typeof(SO_Stat_Integer), true, true, true, false);

            m_Root.Add(m_SubTabContent);
        }

        protected override void OnSubTabChanged(Type _ModuleType, bool _ShowAddAndRemove, bool _LoadSubTypes, bool _ShowInspectorPanel, bool _LoadLocalFiles)
        {
            m_SubTabContent.Clear();

            MethodInfo method = typeof(StatModuleTab).GetMethod("LoadSubTabHierarchy", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo generic = method.MakeGenericMethod(_ModuleType);
            generic.Invoke(this, new object[] { _ShowAddAndRemove, _ShowInspectorPanel, _LoadSubTypes, _LoadLocalFiles });
        }
    }
}