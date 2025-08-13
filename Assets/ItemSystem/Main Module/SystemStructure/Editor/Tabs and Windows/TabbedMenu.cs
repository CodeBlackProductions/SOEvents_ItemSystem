using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// UI element that allows for the creation of a menu structure with multiple tabs.
    /// </summary>
    public class TabbedMenu : VisualElement
    {
        private Button m_CurrentSelection = null;

        public TabbedMenu(
            KeyValuePair<string, System.Type>[] _Tabs,
            System.Action<System.Type, bool, bool, bool, bool, int> _OnTabChanged,
            bool _ShowAddAndRemove, bool _LoadSubTypes, bool _ShowInspectorPanel,
            bool _LoadLocalFiles,
            float _ButtonMinHeight = 0,
            float _ButtonMinWidth = 0,
            int _MainTabColor = -1
            )
        {
            var tabContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            tabContainer.style.paddingTop = 5;
            int index = -1;

            StyleSheet tabButtonStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ItemSystem/Main Module/SystemStructure/Editor/Tabs and Windows/TabButton.uss");
            tabContainer.styleSheets.Add(tabButtonStyle);

            foreach (KeyValuePair<string, System.Type> tab in _Tabs)
            {
                index++;

                var tabButton = new Button(() =>
                {
                    _OnTabChanged?.Invoke(tab.Value, _ShowAddAndRemove, _LoadSubTypes, _ShowInspectorPanel, _LoadLocalFiles, _MainTabColor);
                })
                { text = tab.Key };

                tabButton.AddToClassList("tab-button");
                tabButton.AddToClassList($"tab-c{index}");

                if (_ButtonMinHeight > 0)
                {
                    tabButton.style.minHeight = _ButtonMinHeight;
                }
                if (_ButtonMinWidth > 0)
                {
                    tabButton.style.minWidth = _ButtonMinWidth;
                }

                tabContainer.Add(tabButton);
            }

            Add(tabContainer);
        }

        public TabbedMenu(
            string[] _TabNames,
            System.Action<string, int> _OnTabChanged,
            float _ButtonMinHeight = 0,
            float _ButtonMinWidth = 0
            )
        {
            var tabContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            tabContainer.style.paddingTop = 5;
            int index = -1;

            StyleSheet tabButtonStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ItemSystem/Main Module/SystemStructure/Editor/Tabs and Windows/TabButton.uss");
            tabContainer.styleSheets.Add(tabButtonStyle);

            foreach (var tabName in _TabNames)
            {
                index++;

                var tabButton = new Button(() => _OnTabChanged?.Invoke(tabName, index)) { text = tabName };

                if (_ButtonMinHeight > 0)
                {
                    tabButton.style.minHeight = _ButtonMinHeight;
                }
                if (_ButtonMinWidth > 0)
                {
                    tabButton.style.minWidth = _ButtonMinWidth;
                }

                tabButton.AddToClassList("tab-button");
                tabButton.AddToClassList($"tab-c{index}");

                tabContainer.Add(tabButton);
            }
            Add(tabContainer);
        }
    }
}