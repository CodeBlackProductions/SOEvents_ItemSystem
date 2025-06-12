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

        [MenuItem("Tools/Item System")]
        public static void ShowWindow()
        {
            ItemSystemEditor window = GetWindow<ItemSystemEditor>("Item System");
            window.minSize = new Vector2(800, 500);

            AssetDatabase.SaveAssets();
        }

        public void CreateGUI()
        {
            m_Root = rootVisualElement;

            m_MainTabMenu = new TabbedMenu(new string[] { "ItemModules", "ItemContainers", "StatsModules", "ToolTips", "FileManager", "Tags", "Settings" }, OnMainTabChanged);
            m_MainTabContent = new VisualElement();

            m_MainTabContent.style.flexDirection = FlexDirection.Column;
            m_MainTabContent.style.flexGrow = 1;

            m_Root.Add(m_MainTabMenu);

            OnMainTabChanged("ItemModules");

            m_Root.Add(m_MainTabContent);
        }

        private void OnMainTabChanged(string _TabName)
        {
            m_MainTabContent.Clear();
            m_CurrentMainTab = (EMainTabType)System.Enum.Parse(typeof(EMainTabType), _TabName);

            LoadMainTabHierarchy(m_CurrentMainTab);
        }

        private void LoadMainTabHierarchy(EMainTabType _MainTabType)
        {
            switch (_MainTabType)
            {
                case EMainTabType.Settings:
                    m_MainTabContent?.Clear();
                    m_MainTabContent.Add(new SettingsTab(m_InspectorValueChangeCallback));
                    break;

                case EMainTabType.ItemModules:
                    m_MainTabContent?.Clear();
                    m_MainTabContent.Add(new ItemModuleTab(m_InspectorValueChangeCallback, m_TreeviewSelectionChangeCallback));
                    break;

                case EMainTabType.ItemContainers:
                    m_MainTabContent?.Clear();
                    m_MainTabContent.Add(new ItemContainerTab(m_InspectorValueChangeCallback, m_TreeviewSelectionChangeCallback));
                    break;

                case EMainTabType.StatsModules:

                    m_MainTabContent?.Clear();
                    m_MainTabContent.Add(new StatModuleTab(m_InspectorValueChangeCallback, m_TreeviewSelectionChangeCallback));
                    break;

                case EMainTabType.FileManager:

                    m_MainTabContent?.Clear();
                    m_MainTabContent.Add(new FileManagerTab(m_InspectorValueChangeCallback, m_TreeviewSelectionChangeCallback, m_LocalFileTreeviewSelectionChangeCallback));
                    break;

                case EMainTabType.Tags:
                    m_MainTabContent?.Clear();
                    m_MainTabContent.Add(new TagManagerTab(m_InspectorValueChangeCallback, m_TreeviewSelectionChangeCallback));
                    break;
                case EMainTabType.ToolTips:
                    m_MainTabContent?.Clear();
                    m_MainTabContent.Add(new ToolTipManagerTab(m_InspectorValueChangeCallback, m_TreeviewSelectionChangeCallback));
                    break;

                default:
                    break;
            }
        }

        private enum EMainTabType
        {
            Settings,
            ItemModules,
            ItemContainers,
            StatsModules,
            ToolTips,
            Tags,
            FileManager
        }
    }
}