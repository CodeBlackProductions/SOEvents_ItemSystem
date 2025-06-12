using ItemSystem.SubModules;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Tab for managing tags in the Item System.
    /// </summary>
    public class ToolTipManagerTab : TabBase
    {
        public ToolTipManagerTab(Action<bool> _InspectorValueChangeCallback, Action<IEnumerable<System.Object>, bool, bool> _TreeviewSelectionChangeCallback)
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
            m_SubTabContent?.Clear();

            m_SubTabContent = new VisualElement();
            m_SubTabInspectorPanel = new InspectorPanel();

            m_SubTabContent.style.flexDirection = FlexDirection.Row;
            m_SubTabContent.style.flexGrow = 1;

            LoadFilterPanel();
            LoadSubTabHierarchy<SO_ToolTip>(true, true, false, false);

            m_Root.Add(m_SubTabContent);
        }

        protected override void OnSubTabChanged(Type _ModuleType, bool _ShowAddAndRemove, bool _LoadSubTypes, bool _ShowInspectorPanel, bool _LoadLocalFiles)
        {

        }
    }
}