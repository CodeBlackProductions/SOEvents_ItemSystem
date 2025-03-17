using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemSystemEditor : EditorWindow
{
    private VisualElement m_Root;
    private TabbedMenu m_TabMenu;
    private VisualElement m_TabContent;
    private TreeView m_TabHierarchy;
    private InspectorPanel m_InspectorPanel;

    [MenuItem("Tools/Item System")]
    public static void ShowWindow()
    {
        ItemSystemEditor window = GetWindow<ItemSystemEditor>("Item System");
        window.minSize = new Vector2(600, 400);
    }

    public void CreateGUI()
    {
        m_Root = rootVisualElement;

        m_TabMenu = new TabbedMenu(new string[] { "Items", "Classes", "Types", "Effects", "Triggers" }, OnTabChanged);
        m_TabContent = new VisualElement();
        m_TabHierarchy = new TreeView();
        m_InspectorPanel = new InspectorPanel();

        m_TabContent.style.flexDirection = FlexDirection.Row;

        m_Root.Add(m_TabMenu);
        m_Root.Add(m_TabContent);

        ShowTabHierarchy(ETabType.Items);

        m_TabContent.Add(m_InspectorPanel);

        m_TabHierarchy?.CollapseAll();
    }

    private void OnEnable()
    {
        m_TabHierarchy?.CollapseAll();
    }

    private void OnTabChanged(string _TabName)
    {
        m_TabContent.Clear();

        ShowTabHierarchy((ETabType)System.Enum.Parse(typeof(ETabType), _TabName));

        m_TabContent.Add(m_InspectorPanel);
    }

    private void ShowTabHierarchy(ETabType _ModuleType)
    {
        m_TabHierarchy.selectionChanged -= OnTreeViewSelectionChanged;
        m_TabHierarchy = new TreeView();
        m_TabHierarchy.selectionChanged += OnTreeViewSelectionChanged;
        m_TabHierarchy.style.flexGrow = 1;

        List<TreeViewItemData<string>> treeItems = LoadTabHierarchy(_ModuleType);
        m_TabHierarchy.SetRootItems(treeItems);
        m_TabContent.Add(m_TabHierarchy);
    }

    private List<TreeViewItemData<string>> LoadTabHierarchy(ETabType _ModuleType)
    {
        List<TreeViewItemData<string>> treeItems = new List<TreeViewItemData<string>>();

        switch (_ModuleType)
        {
            case ETabType.Items:
                treeItems = LoadItems();
                break;

            case ETabType.Classes:
                treeItems = LoadClasses();
                break;

            case ETabType.Types:
                treeItems = LoadTypes();
                break;

            case ETabType.Effects:
                treeItems = LoadEffects();
                break;

            case ETabType.Triggers:
                treeItems = LoadTriggers();
                break;

            default:
                Debug.LogError("Invalid TabType");
                break;
        }

        return treeItems;
    }

    private List<TreeViewItemData<string>> LoadItems()
    {
        List<TreeViewItemData<string>> treeItems = new List<TreeViewItemData<string>>();
        int idCounter = 0;

        var items = ItemEditorAssetLoader.LoadAssetsByType<SO_Item>();
        var classes = ItemEditorAssetLoader.LoadAssetsByType<SO_Item_Class>();
        var types = ItemEditorAssetLoader.LoadAssetsByType<SO_Class_Type>();
        var effects = ItemEditorAssetLoader.LoadAssetsByType<SO_Item_Effect>();
        var triggers = ItemEditorAssetLoader.LoadAssetsByType<SO_Effect_Trigger>();

        foreach (var item in items)
        {
            List<TreeViewItemData<string>> itemChildren = new List<TreeViewItemData<string>>();

            foreach (var cls in classes.Where(c => item.Class == c))
            {
                List<TreeViewItemData<string>> classChildren = new List<TreeViewItemData<string>>();

                foreach (var type in types.Where(t => cls.Types.Contains(t)))
                {
                    classChildren.Add(new TreeViewItemData<string>(idCounter++, type.name));
                }

                itemChildren.Add(new TreeViewItemData<string>(idCounter++, cls.name, classChildren));
            }

            foreach (var effect in effects.Where(e => item.Effects.Contains(e)))
            {
                List<TreeViewItemData<string>> effectChildren = new List<TreeViewItemData<string>>();

                foreach (var trigger in triggers.Where(t => effect.Trigger == t))
                {
                    effectChildren.Add(new TreeViewItemData<string>(idCounter++, trigger.name));
                }

                itemChildren.Add(new TreeViewItemData<string>(idCounter++, effect.name, effectChildren));
            }

            treeItems.Add(new TreeViewItemData<string>(idCounter++, item.name, itemChildren));
        }

        return treeItems;
    }

    private List<TreeViewItemData<string>> LoadClasses()
    {
        List<TreeViewItemData<string>> treeItems = new List<TreeViewItemData<string>>();
        int idCounter = 0;

        var classes = ItemEditorAssetLoader.LoadAssetsByType<SO_Item_Class>();
        var types = ItemEditorAssetLoader.LoadAssetsByType<SO_Class_Type>();

        foreach (var cls in classes)
        {
            List<TreeViewItemData<string>> classChildren = new List<TreeViewItemData<string>>();

            foreach (var type in types.Where(t => cls.Types.Contains(t)))
            {
                classChildren.Add(new TreeViewItemData<string>(idCounter++, type.name));
            }

            treeItems.Add(new TreeViewItemData<string>(idCounter++, cls.name, classChildren));
        }

        return treeItems;
    }

    private List<TreeViewItemData<string>> LoadTypes()
    {
        List<TreeViewItemData<string>> treeItems = new List<TreeViewItemData<string>>();
        int idCounter = 0;

        var types = ItemEditorAssetLoader.LoadAssetsByType<SO_Class_Type>();
        foreach (var type in types)
        {
            treeItems.Add(new TreeViewItemData<string>(idCounter++, type.name));
        }

        return treeItems;
    }

    private List<TreeViewItemData<string>> LoadEffects()
    {
        List<TreeViewItemData<string>> treeItems = new List<TreeViewItemData<string>>();
        int idCounter = 0;

        var effects = ItemEditorAssetLoader.LoadAssetsByType<SO_Item_Effect>();
        var triggers = ItemEditorAssetLoader.LoadAssetsByType<SO_Effect_Trigger>();

        foreach (var effect in effects)
        {
            List<TreeViewItemData<string>> effectChildren = new List<TreeViewItemData<string>>();

            foreach (var trigger in triggers.Where(t => effect.Trigger == t))
            {
                effectChildren.Add(new TreeViewItemData<string>(idCounter++, trigger.name));
            }

            treeItems.Add(new TreeViewItemData<string>(idCounter++, effect.name, effectChildren));
        }

        return treeItems;
    }

    private List<TreeViewItemData<string>> LoadTriggers()
    {
        List<TreeViewItemData<string>> treeItems = new List<TreeViewItemData<string>>();
        int idCounter = 0;

        var triggers = ItemEditorAssetLoader.LoadAssetsByType<SO_Effect_Trigger>();
        foreach (var trigger in triggers)
        {
            treeItems.Add(new TreeViewItemData<string>(idCounter++, trigger.name));
        }

        return treeItems;
    }

    private enum ETabType
    {
        Items,
        Classes,
        Types,
        Effects,
        Triggers
    }

    private void OnTreeViewSelectionChanged(IEnumerable<object> _SelectedItems)
    {
        foreach (var selectedItem in _SelectedItems)
        {
            ShowInspectorPanel(selectedItem.ToString());
        }
    }

    private void ShowInspectorPanel(string _ItemSOName)
    {
        ScriptableObject so = ItemEditorAssetLoader.LoadAssetByName<ScriptableObject>(_ItemSOName);
        m_InspectorPanel.Show(so);
    }
}