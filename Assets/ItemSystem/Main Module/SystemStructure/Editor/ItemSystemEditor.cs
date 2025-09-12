using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Main editor window for the Item System.
    /// </summary>
    public class ItemSystemEditor : EditorWindow
    {
        private VisualElement m_Root;
        private TabbedMenu m_MainTabMenu;
        private VisualElement m_MainTabContent;
        private EMainTabType m_CurrentMainTab;

        private Action<bool> m_InspectorValueChangeCallback;
        private Action<IEnumerable<System.Object>, bool, bool> m_TreeviewSelectionChangeCallback;
        private Action<IEnumerable<System.Object>> m_LocalFileTreeviewSelectionChangeCallback;

        [MenuItem("Tools/Item System/Item Editor")]
        public static void ShowWindow()
        {
            ItemSystemEditor window = GetWindow<ItemSystemEditor>("Item System Editor");
            window.minSize = new Vector2(800, 500);


            AssetDatabase.SaveAssets();
        }

        public void CreateGUI()
        {
            m_Root = rootVisualElement;

            m_MainTabMenu = new TabbedMenu(
                new string[] { "ItemModules", "StatsModules", "ToolTips", "Tags", "ItemContainers" },
                OnMainTabChanged,
                40
            );
            m_MainTabMenu.style.minHeight = 45;
            m_MainTabMenu.style.borderBottomWidth = 2;
            m_MainTabMenu.style.borderBottomColor = new StyleColor(Color.grey);
            m_MainTabMenu.style.paddingBottom = 5;

            m_MainTabContent = new VisualElement();

            m_MainTabContent.style.flexDirection = FlexDirection.Column;
            m_MainTabContent.style.flexGrow = 1;

            m_Root.Add(m_MainTabMenu);

            OnMainTabChanged("ItemModules", 0);

            m_Root.Add(m_MainTabContent);
        }

        private void OnMainTabChanged(string _TabName, int _BGColor)
        {
            m_MainTabContent.Clear();
            m_CurrentMainTab = (EMainTabType)System.Enum.Parse(typeof(EMainTabType), _TabName);

            StyleSheet styleSheet = UI_Styles_Lib.GetUIStyles();

            if (!m_MainTabMenu.styleSheets.Contains(styleSheet))
            {
                m_MainTabMenu.styleSheets.Add(styleSheet);
            }
            m_MainTabMenu.ClearClassList();
            m_MainTabMenu.AddToClassList($"tab-c-{_BGColor}-dark");

            LoadMainTabHierarchy(m_CurrentMainTab, _BGColor);
        }

        private void LoadMainTabHierarchy(EMainTabType _MainTabType, int _BGColor)
        {
            VisualElement temp = null;

            switch (_MainTabType)
            {
                case EMainTabType.ItemModules:
                    m_MainTabContent?.Clear();
                    temp = new ItemModuleTab(m_InspectorValueChangeCallback, m_TreeviewSelectionChangeCallback, _BGColor);
                    temp.style.flexGrow = 1;
                    m_MainTabContent.Add(temp);
                    break;

                case EMainTabType.ItemContainers:
                    m_MainTabContent?.Clear();
                    temp = new ItemContainerTab(m_InspectorValueChangeCallback, m_TreeviewSelectionChangeCallback, _BGColor);
                    temp.style.flexGrow = 1;
                    m_MainTabContent.Add(temp);
                    break;

                case EMainTabType.StatsModules:

                    m_MainTabContent?.Clear();
                    temp = new StatModuleTab(m_InspectorValueChangeCallback, m_TreeviewSelectionChangeCallback, _BGColor);
                    temp.style.flexGrow = 1;
                    m_MainTabContent.Add(temp);
                    break;

                case EMainTabType.Tags:
                    m_MainTabContent?.Clear();
                    temp = new TagManagerTab(m_InspectorValueChangeCallback, m_TreeviewSelectionChangeCallback, _BGColor);
                    temp.style.flexGrow = 1;
                    m_MainTabContent.Add(temp);
                    break;

                case EMainTabType.ToolTips:
                    m_MainTabContent?.Clear();
                    temp = new ToolTipManagerTab(m_InspectorValueChangeCallback, m_TreeviewSelectionChangeCallback, _BGColor);
                    temp.style.flexGrow = 1;
                    m_MainTabContent.Add(temp);
                    break;

                default:
                    break;
            }
        }

        private enum EMainTabType
        {
            ItemModules,
            ItemContainers,
            StatsModules,
            ToolTips,
            Tags
        }
    }
}