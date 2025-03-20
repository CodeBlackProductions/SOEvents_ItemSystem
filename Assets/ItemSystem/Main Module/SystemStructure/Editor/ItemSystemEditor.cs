using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemSystemEditor : EditorWindow
{
    private VisualElement m_Root;
    private TabbedMenu m_TabMenu;
    private VisualElement m_TabContent;
    private InspectorPanel m_InspectorPanel;
    private ETabType m_CurrentTab;

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

        m_TabMenu = new TabbedMenu(new string[] { "Items", "Classes", "Types", "Effects", "Triggers", "Settings" }, OnTabChanged);
        m_TabContent = new VisualElement();
        m_InspectorPanel = new InspectorPanel();

        m_TreeviewSelectionChangeCallback += OnTreeViewSelectionChanged;

        m_TabContent.style.flexDirection = FlexDirection.Row;
        m_TabContent.style.flexGrow = 1;

        m_Root.Add(m_TabMenu);

        LoadTabHierarchy(ETabType.Items);
        m_TabContent.Add(m_InspectorPanel);

        m_Root.Add(m_TabContent);
    }

    private void OnTabChanged(string _TabName)
    {
        m_TabContent.Clear();
        m_CurrentTab = (ETabType)System.Enum.Parse(typeof(ETabType), _TabName);

        if (m_CurrentTab != ETabType.Settings)
        {
            LoadTabHierarchy(m_CurrentTab);
            m_TabContent.Add(m_InspectorPanel);
        }
        else
        {
        }
    }

    private void LoadTabHierarchy(ETabType _ModuleType)
    {
        switch (_ModuleType)
        {
            case ETabType.Items:
                DynamicItemTreeView<SO_Item> itemsTreeView = new DynamicItemTreeView<SO_Item>(m_TreeviewSelectionChangeCallback, true);
                m_InspectorValueChangeCallback += itemsTreeView.RefreshTreeView;
                itemsTreeView.style.flexGrow = 1;
                m_TabContent.Add(itemsTreeView);

                break;

            case ETabType.Classes:
                DynamicItemTreeView<SO_Item_Class> classTreeView = new DynamicItemTreeView<SO_Item_Class>(m_TreeviewSelectionChangeCallback, true);
                m_InspectorValueChangeCallback += classTreeView.RefreshTreeView;
                classTreeView.style.flexGrow = 1;
                m_TabContent.Add(classTreeView);

                break;

            case ETabType.Types:
                DynamicItemTreeView<SO_Class_Type> typeTreeView = new DynamicItemTreeView<SO_Class_Type>(m_TreeviewSelectionChangeCallback, true);
                m_InspectorValueChangeCallback += typeTreeView.RefreshTreeView;
                typeTreeView.style.flexGrow = 1;
                m_TabContent.Add(typeTreeView);

                break;

            case ETabType.Effects:
                DynamicItemTreeView<SO_Item_Effect> effectTreeView = new DynamicItemTreeView<SO_Item_Effect>(m_TreeviewSelectionChangeCallback, true);
                m_InspectorValueChangeCallback += effectTreeView.RefreshTreeView;
                effectTreeView.style.flexGrow = 1;
                m_TabContent.Add(effectTreeView);

                break;

            case ETabType.Triggers:
                DynamicItemTreeView<SO_Effect_Trigger> triggerTreeView = new DynamicItemTreeView<SO_Effect_Trigger>(m_TreeviewSelectionChangeCallback, true);
                m_InspectorValueChangeCallback += triggerTreeView.RefreshTreeView;
                triggerTreeView.style.flexGrow = 1;
                m_TabContent.Add(triggerTreeView);

                break;

            default:
                Debug.LogError("Invalid TabType");
                break;
        }
    }

    private enum ETabType
    {
        Items,
        Classes,
        Types,
        Effects,
        Triggers,
        Settings
    }

    private void OnTreeViewSelectionChanged(IEnumerable<object> _SelectedItems)
    {
        foreach (var selectedItem in _SelectedItems)
        {
            ShowInspectorPanel(selectedItem.ToString(), m_InspectorValueChangeCallback);
        }
    }

    private void ShowInspectorPanel(string _ItemSOName, Action _InspectorValueChangeCallback)
    {
        ScriptableObject so = ItemEditor_AssetLoader.LoadAssetByName<ScriptableObject>(_ItemSOName);
        m_InspectorPanel.Show(so, _InspectorValueChangeCallback);
    }
}