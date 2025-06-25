using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Tab for displaying and editing settings of the Item System editor.
    /// </summary>
    public class SettingsTab : TabBase
    {
        public SettingsTab(Action<bool> _InspectorValueChangeCallback)
        {
            m_InspectorValueChangeCallback = _InspectorValueChangeCallback;

            m_Root = new VisualElement();
            m_Root.style.flexDirection = FlexDirection.Column;
            m_Root.style.flexGrow = 1;

            LoadHierarchy();

            Add(m_Root);
        }

        protected override void LoadHierarchy()
        {
            m_Root?.Clear();

            m_SubTabInspectorPanel = new InspectorPanel();
            ShowInspectorPanel("EditorSettings", m_InspectorValueChangeCallback);

            m_Root.Add(m_SubTabInspectorPanel);
        }

        protected override void OnSubTabChanged(System.Type _ModuleType, bool _ShowAddAndRemove, bool _LoadSubTypes, bool _ShowInspectorPanel, bool _LoadLocalFiles)
        {
            return;
        }
    }
}