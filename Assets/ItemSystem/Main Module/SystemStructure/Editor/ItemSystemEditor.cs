using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    private EMainTabType m_CurrentMainTab;

    private Action<bool> m_InspectorValueChangeCallback;
    private Action<IEnumerable<System.Object>, bool> m_TreeviewSelectionChangeCallback;
    private Action<IEnumerable<System.Object>> m_LocalFileTreeviewSelectionChangeCallback;

    private List<ScriptableObject> m_LocalFileItems = new List<ScriptableObject>();
    private Button m_BTN_SaveToFile;
    private Button m_BTN_LoadIntoAssets;

    [MenuItem("Tools/Item System")]
    public static void ShowWindow()
    {
        ItemSystemEditor window = GetWindow<ItemSystemEditor>("Item System");
        window.minSize = new Vector2(700, 400);
    }

    public void CreateGUI()
    {
        m_Root = rootVisualElement;

        m_MainTabMenu = new TabbedMenu(new string[] { "ItemModules", "ItemContainers", "StatsModules", "FileManager", "Settings" }, OnMainTabChanged);
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

    private void OnSubTabChanged(Type _ModuleType, bool _ShowAddAndRemove, bool _LoadSubTypes, bool _ShowInspectorPanel, bool _LoadLocalFiles)
    {
        m_SubTabContent.Clear();
        MethodInfo method = typeof(ItemSystemEditor).GetMethod("LoadSubTabHierarchy", BindingFlags.NonPublic | BindingFlags.Instance);
        MethodInfo generic = method.MakeGenericMethod(_ModuleType);
        generic.Invoke(this, new object[] { _ShowAddAndRemove, _LoadSubTypes, _ShowInspectorPanel, _LoadLocalFiles });
    }

    private void LoadMainTabHierarchy(EMainTabType _MainTabType)
    {
        switch (_MainTabType)
        {
            case EMainTabType.Settings:
                m_MainTabContent?.Clear();
                m_SubTabContent?.Clear();
                m_SubTabInspectorPanel = new InspectorPanel();
                ShowInspectorPanel("EditorSettings", m_InspectorValueChangeCallback);
                m_MainTabContent.Add(m_SubTabInspectorPanel);
                break;

            case EMainTabType.ItemModules:
                m_MainTabContent?.Clear();
                m_SubTabContent?.Clear();

                m_SubTabMenu = new TabbedMenu(new KeyValuePair<string, Type>[]
                {
                    new KeyValuePair<string, Type>("Items", typeof(SO_Item)),
                    new KeyValuePair<string, Type>("Classes", typeof(SO_Item_Class)),
                    new KeyValuePair<string, Type>("Types", typeof(SO_Class_Type)),
                    new KeyValuePair<string, Type>("Effects", typeof(SO_Item_Effect)),
                    new KeyValuePair<string, Type>("Triggers", typeof(SO_Effect_Trigger))
                }, OnSubTabChanged, true, true, true, false);

                m_SubTabContent = new VisualElement();
                m_SubTabInspectorPanel = new InspectorPanel();

                m_SubTabContent.style.flexDirection = FlexDirection.Row;
                m_SubTabContent.style.flexGrow = 1;

                m_MainTabContent.Add(m_SubTabMenu);

                LoadSubTabHierarchy<SO_Item>(true, true, true, false);
                m_SubTabContent.Add(m_SubTabInspectorPanel);

                m_MainTabContent.Add(m_SubTabContent);
                break;

            case EMainTabType.ItemContainers:
                m_MainTabContent?.Clear();
                m_SubTabContent?.Clear();
                m_SubTabMenu = new TabbedMenu(new KeyValuePair<string, Type>[]
                {
                    new KeyValuePair<string, Type>("ItemSlots", typeof(SO_ItemSlot)),
                    new KeyValuePair<string, Type>("ItemPools", null),
                }, OnSubTabChanged, true, true, true, false);
                m_SubTabContent = new VisualElement();
                m_SubTabInspectorPanel = new InspectorPanel();

                m_SubTabContent.style.flexDirection = FlexDirection.Row;
                m_SubTabContent.style.flexGrow = 1;

                m_MainTabContent.Add(m_SubTabMenu);

                LoadSubTabHierarchy<SO_ItemSlot>(true, true, true, false);
                m_SubTabContent.Add(m_SubTabInspectorPanel);

                m_MainTabContent.Add(m_SubTabContent);
                break;

            case EMainTabType.StatsModules:

                m_MainTabContent?.Clear();
                m_SubTabContent?.Clear();
                m_SubTabContent = new VisualElement();
                m_SubTabInspectorPanel = new InspectorPanel();

                m_SubTabContent.style.flexDirection = FlexDirection.Row;
                m_SubTabContent.style.flexGrow = 1;

                LoadSubTabHierarchy<SO_Stat>(true, true, true, false);
                m_SubTabContent.Add(m_SubTabInspectorPanel);

                m_MainTabContent.Add(m_SubTabContent);
                break;

            case EMainTabType.FileManager:

                m_MainTabContent?.Clear();
                m_SubTabContent?.Clear();
                m_SubTabMenu = new TabbedMenu(new KeyValuePair<string, Type>[]
               {
                    new KeyValuePair<string, Type>("Items", typeof(SO_Item)),
                    new KeyValuePair<string, Type>("Classes", typeof(SO_Item_Class)),
                    new KeyValuePair<string, Type>("Types", typeof(SO_Class_Type)),
                    new KeyValuePair<string, Type>("Effects", typeof(SO_Item_Effect)),
                    new KeyValuePair<string, Type>("Triggers", typeof(SO_Effect_Trigger)),
                    new KeyValuePair<string, Type>("ItemSlots", typeof(SO_ItemSlot)),
                    new KeyValuePair<string, Type>("ItemPools", null),
                    new KeyValuePair<string, Type>("Stats", typeof(SO_Stat)),
               }, OnSubTabChanged, false, false, false, true);
                m_SubTabContent = new VisualElement();

                m_SubTabContent.style.flexDirection = FlexDirection.Row;
                m_SubTabContent.style.flexGrow = 1;

                m_MainTabContent.Add(m_SubTabMenu);

                LoadSubTabHierarchy<SO_Item>(false, false, false, true);

                m_MainTabContent.Add(m_SubTabContent);
                break;

            default:
                break;
        }
    }

    private void LoadSubTabHierarchy<T>(bool _ShowAddAndRemove, bool _ShowInspectorPanel, bool _LoadSubtypes, bool _LoadLocalFiles) where T : ScriptableObject
    {
        CreateTreeview<T>(_ShowAddAndRemove, _LoadSubtypes, _LoadLocalFiles);
        m_TreeviewSelectionChangeCallback += OnTreeViewSelectionChanged;

        if (_LoadLocalFiles)
        {
            m_LocalFileItems = ItemEditor_FileManager.LoadModulesFromFiles(typeof(T));
            CreateTreeview<ScriptableObject>(m_LocalFileItems, false);
            m_LocalFileTreeviewSelectionChangeCallback += OnLocalFileTreeviewSelectionChanged;
        }

        if (_ShowInspectorPanel)
        {
            m_SubTabContent.Add(m_SubTabInspectorPanel);
        }
    }

    private void CreateTreeview<T>(bool _ShowAddAndRemove, bool _LoadSubTypes, bool _ShowSaveToFile) where T : ScriptableObject
    {
        DynamicItemTreeView<T> treeView = new DynamicItemTreeView<T>(m_TreeviewSelectionChangeCallback, _ShowAddAndRemove, _LoadSubTypes, _ShowSaveToFile);
        m_InspectorValueChangeCallback += treeView.RefreshTreeView;
        treeView.style.flexGrow = 1;
        m_SubTabContent.Add(treeView);
    }

    private void CreateTreeview<T>(List<ScriptableObject> _Items, bool _SaveToFile) where T : ScriptableObject
    {
        DynamicItemTreeView<T> treeView = new DynamicItemTreeView<T>(_Items, m_LocalFileTreeviewSelectionChangeCallback);
        m_InspectorValueChangeCallback += treeView.RefreshTreeView;
        treeView.style.flexGrow = 1;
        m_SubTabContent.Add(treeView);
    }

    private void OnTreeViewSelectionChanged(IEnumerable<object> _SelectedItems, bool _ShowSaveToFile)
    {
        if (m_MainTabContent.Contains(m_BTN_SaveToFile))
        {
            m_MainTabContent.Remove(m_BTN_SaveToFile);
        }
        if (m_MainTabContent.Contains(m_BTN_LoadIntoAssets))
        {
            m_MainTabContent.Remove(m_BTN_LoadIntoAssets);
        }

        foreach (var selectedItem in _SelectedItems)
        {
            if (selectedItem is TreeViewEntryData data)
            {
                if (_ShowSaveToFile)
                {
                    ScriptableObject so = ItemEditor_AssetLoader.LoadAssetByName<ScriptableObject>(data.FileName);

                    m_BTN_SaveToFile = new Button(() =>
                    {
                        ItemEditor_FileManager.SaveModuleToFile(so);
                    });
                    m_BTN_SaveToFile.text = $"Save {(so as IItemModule).ModuleName} to JSON";

                    m_MainTabContent.Add(m_BTN_SaveToFile);
                }

                ShowInspectorPanel(data.FileName, m_InspectorValueChangeCallback);
            }
        }
    }

    private void OnLocalFileTreeviewSelectionChanged(IEnumerable<object> _SelectedItems)
    {
        if (m_MainTabContent.Contains(m_BTN_SaveToFile))
        {
            m_MainTabContent.Remove(m_BTN_SaveToFile);
        }
        if (m_MainTabContent.Contains(m_BTN_LoadIntoAssets))
        {
            m_MainTabContent.Remove(m_BTN_LoadIntoAssets);
        }

        foreach (var selectedItem in _SelectedItems)
        {
            if (selectedItem is TreeViewEntryData data)
            {
                ScriptableObject localFileItem = m_LocalFileItems.FirstOrDefault(item => item.name == data.FileName);
                if (localFileItem != null)
                {
                    m_BTN_LoadIntoAssets = new Button(() =>
                    {
                        ItemEditor_InstanceManager.CreateInstance(localFileItem, localFileItem.GetType());
                    });
                    m_BTN_LoadIntoAssets.text = $"Load {(localFileItem as IItemModule).ModuleName} into Assetfolder";

                    m_MainTabContent.Add(m_BTN_LoadIntoAssets);
                }
            }
        }
    }

    private void ShowInspectorPanel(string _FileName, Action<bool> _InspectorValueChangeCallback)
    {
        ScriptableObject so = ItemEditor_AssetLoader.LoadAssetByName<ScriptableObject>(_FileName);
        m_SubTabInspectorPanel.Show(so, _InspectorValueChangeCallback);
    }

    private enum EMainTabType
    {
        Settings,
        ItemModules,
        ItemContainers,
        StatsModules,
        FileManager
    }
}