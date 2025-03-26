using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemSystemEditor : EditorWindow
{
    private VisualElement m_Root;
    private TabbedMenu m_MainTabMenu;
    private TabbedMenu m_SubTabMenu;
    private VisualElement m_MainTabContent;
    private VisualElement m_SubTabContent;
    private InspectorPanel m_SubTabInspectorPanel;
    private ESubTabType m_CurrentSubTab;
    private EMainTabType m_CurrentMainTab;

    private Action m_InspectorValueChangeCallback;
    private Action<IEnumerable<System.Object>> m_TreeviewSelectionChangeCallback;

    [MenuItem("Tools/Item System")]
    public static void ShowWindow()
    {
        ItemSystemEditor window = GetWindow<ItemSystemEditor>("Item System");
        window.minSize = new Vector2(700, 400);
    }

    public void CreateGUI()
    {
        m_Root = rootVisualElement;

        m_MainTabMenu = new TabbedMenu(new string[] { "ItemModules", "ItemContainers", "StatsModules", "Settings" }, OnMainTabChanged);
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

    private void OnSubTabChanged(string _TabName)
    {
        m_SubTabContent.Clear();
        m_CurrentSubTab = (ESubTabType)System.Enum.Parse(typeof(ESubTabType), _TabName);

        LoadSubTabHierarchy(m_CurrentSubTab);
    }

    private void LoadMainTabHierarchy(EMainTabType _MainTabType)
    {
        switch (_MainTabType)
        {
            case EMainTabType.Settings:
                m_MainTabContent.Clear();
                m_SubTabInspectorPanel = new InspectorPanel();
                ShowInspectorPanel("EditorSettings", m_InspectorValueChangeCallback);
                m_MainTabContent.Add(m_SubTabInspectorPanel);
                break;

            case EMainTabType.ItemModules:
                m_SubTabMenu = new TabbedMenu(new string[] { "Items", "Classes", "Types", "Effects", "Triggers" }, OnSubTabChanged);
                m_SubTabContent = new VisualElement();
                m_SubTabInspectorPanel = new InspectorPanel();

                m_TreeviewSelectionChangeCallback += OnTreeViewSelectionChanged;

                m_SubTabContent.style.flexDirection = FlexDirection.Row;
                m_SubTabContent.style.flexGrow = 1;

                m_MainTabContent.Add(m_SubTabMenu);

                LoadSubTabHierarchy(ESubTabType.Items);
                m_SubTabContent.Add(m_SubTabInspectorPanel);

                m_MainTabContent.Add(m_SubTabContent);
                break;

            case EMainTabType.ItemContainers:
                m_SubTabMenu = new TabbedMenu(new string[] { "ItemSlots", "ItemPools" }, OnSubTabChanged);
                m_SubTabContent = new VisualElement();
                m_SubTabInspectorPanel = new InspectorPanel();

                m_TreeviewSelectionChangeCallback += OnTreeViewSelectionChanged;

                m_SubTabContent.style.flexDirection = FlexDirection.Row;
                m_SubTabContent.style.flexGrow = 1;

                m_MainTabContent.Add(m_SubTabMenu);

                LoadSubTabHierarchy(ESubTabType.ItemSlots);
                m_SubTabContent.Add(m_SubTabInspectorPanel);

                m_MainTabContent.Add(m_SubTabContent);
                break;

            case EMainTabType.StatsModules:

                m_SubTabContent = new VisualElement();
                m_SubTabInspectorPanel = new InspectorPanel();

                m_TreeviewSelectionChangeCallback += OnTreeViewSelectionChanged;

                m_SubTabContent.style.flexDirection = FlexDirection.Row;
                m_SubTabContent.style.flexGrow = 1;

                LoadSubTabHierarchy(ESubTabType.Stats);
                m_SubTabContent.Add(m_SubTabInspectorPanel);

                m_MainTabContent.Add(m_SubTabContent);
                break;

            default:
                break;
        }
    }

    private void LoadSubTabHierarchy(ESubTabType _ModuleType)
    {
        switch (_ModuleType)
        {
            case ESubTabType.Items:
                DynamicItemTreeView<SO_Item> itemsTreeView = new DynamicItemTreeView<SO_Item>(m_TreeviewSelectionChangeCallback, true);
                m_InspectorValueChangeCallback += itemsTreeView.RefreshTreeView;
                itemsTreeView.style.flexGrow = 1;
                m_SubTabContent.Add(itemsTreeView);

                break;

            case ESubTabType.Classes:
                DynamicItemTreeView<SO_Item_Class> classTreeView = new DynamicItemTreeView<SO_Item_Class>(m_TreeviewSelectionChangeCallback, true);
                m_InspectorValueChangeCallback += classTreeView.RefreshTreeView;
                classTreeView.style.flexGrow = 1;
                m_SubTabContent.Add(classTreeView);

                break;

            case ESubTabType.Types:
                DynamicItemTreeView<SO_Class_Type> typeTreeView = new DynamicItemTreeView<SO_Class_Type>(m_TreeviewSelectionChangeCallback, true);
                m_InspectorValueChangeCallback += typeTreeView.RefreshTreeView;
                typeTreeView.style.flexGrow = 1;
                m_SubTabContent.Add(typeTreeView);

                break;

            case ESubTabType.Effects:
                DynamicItemTreeView<SO_Item_Effect> effectTreeView = new DynamicItemTreeView<SO_Item_Effect>(m_TreeviewSelectionChangeCallback, true);
                m_InspectorValueChangeCallback += effectTreeView.RefreshTreeView;
                effectTreeView.style.flexGrow = 1;
                m_SubTabContent.Add(effectTreeView);

                break;

            case ESubTabType.Triggers:
                DynamicItemTreeView<SO_Effect_Trigger> triggerTreeView = new DynamicItemTreeView<SO_Effect_Trigger>(m_TreeviewSelectionChangeCallback, true);
                m_InspectorValueChangeCallback += triggerTreeView.RefreshTreeView;
                triggerTreeView.style.flexGrow = 1;
                m_SubTabContent.Add(triggerTreeView);

                break;

            case ESubTabType.ItemSlots:
                DynamicItemTreeView<SO_ItemSlot> slotTreeView = new DynamicItemTreeView<SO_ItemSlot>(m_TreeviewSelectionChangeCallback, true);
                m_InspectorValueChangeCallback += slotTreeView.RefreshTreeView;
                slotTreeView.style.flexGrow = 1;
                m_SubTabContent.Add(slotTreeView);
                break;

            case ESubTabType.ItemPools:
                throw new NotImplementedException();
            case ESubTabType.Stats:
                DynamicItemTreeView<SO_Stat> statTreeView = new DynamicItemTreeView<SO_Stat>(m_TreeviewSelectionChangeCallback, true);
                m_InspectorValueChangeCallback += statTreeView.RefreshTreeView;
                statTreeView.style.flexGrow = 1;
                m_SubTabContent.Add(statTreeView);
                break;

            default:
                Debug.LogError("Invalid TabType");
                break;
        }

        m_SubTabContent.Add(m_SubTabInspectorPanel);
    }

    private void OnTreeViewSelectionChanged(IEnumerable<object> _SelectedItems)
    {
        foreach (var selectedItem in _SelectedItems)
        {
            if (selectedItem is TreeViewEntryData data)
            {
                ShowInspectorPanel(data.FileName, m_InspectorValueChangeCallback);
            }
        }
    }

    private void ShowInspectorPanel(string _FileName, Action _InspectorValueChangeCallback)
    {
        ScriptableObject so = ItemEditor_AssetLoader.LoadAssetByName<ScriptableObject>(_FileName);
        m_SubTabInspectorPanel.Show(so, _InspectorValueChangeCallback);
    }

    private enum ESubTabType
    {
        Items,
        Classes,
        Types,
        Effects,
        Triggers,
        ItemSlots,
        ItemPools,
        Stats
    }

    private enum EMainTabType
    {
        Settings,
        ItemModules,
        ItemContainers,
        StatsModules
    }
}