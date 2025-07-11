using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// UI element that allows for the creation of a menu structure with multiple tabs.
    /// </summary>
    public class TabbedMenu : VisualElement
    {
        public TabbedMenu(KeyValuePair<string, System.Type>[] _Tabs, System.Action<System.Type, bool, bool, bool, bool> _OnTabChanged, bool _ShowAddAndRemove, bool _LoadSubTypes, bool _ShowInspectorPanel, bool _LoadLocalFiles)
        {
            var tabContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            foreach (KeyValuePair<string, System.Type> tab in _Tabs)
            {
                var tabButton = new Button(() =>
                {
                    _OnTabChanged?.Invoke(tab.Value, _ShowAddAndRemove, _LoadSubTypes, _ShowInspectorPanel, _LoadLocalFiles);
                })
                { text = tab.Key };
                tabButton.style.flexGrow = 1;
                tabContainer.Add(tabButton);
            }
            Add(tabContainer);
        }

        public TabbedMenu(string[] _TabNames, System.Action<string> _OnTabChanged)
        {
            var tabContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            foreach (var tabName in _TabNames)
            {
                var tabButton = new Button(() => _OnTabChanged?.Invoke(tabName)) { text = tabName };
                tabButton.style.flexGrow = 1;
                tabContainer.Add(tabButton);
            }
            Add(tabContainer);
        }
    }
}