using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemSystemEditor : EditorWindow
{
    private VisualElement root;
    private TabbedMenu tabMenu;
    private VisualElement tabContent;
    private TreeView itemTreeView;
    private InspectorPanel inspectorPanel;

    [MenuItem("Tools/Item System")]
    public static void ShowWindow()
    {
        var window = GetWindow<ItemSystemEditor>("Item System");
        window.minSize = new Vector2(600, 400);
    }

    public void CreateGUI()
    {
        root = rootVisualElement;

        tabMenu = new TabbedMenu(new string[] { "Items", "Classes", "Types", "Effects", "Triggers" }, OnTabChanged);
        tabContent = new VisualElement();
        itemTreeView = new TreeView();
        inspectorPanel = new InspectorPanel();

        tabContent.style.flexDirection = FlexDirection.Row;

        root.Add(tabMenu);
        root.Add(tabContent);

        // Default to Items Tab
        ShowItemTreeView();

        tabContent.Add(inspectorPanel);

        itemTreeView?.CollapseAll();
    }

    private void OnEnable()
    {
        itemTreeView?.CollapseAll();
    }

    private void OnTabChanged(string tabName)
    {
        tabContent.Clear();

        if (tabName == "Items")
        {
            ShowItemTreeView();
        }
        else
        {
            ShowModuleListView(tabName);
        }

        tabContent.Add(inspectorPanel);
    }

    private void ShowItemTreeView()
    {
        itemTreeView.selectionChanged -= OnTreeViewSelectionChanged;
        itemTreeView = new TreeView();
        itemTreeView.selectionChanged += OnTreeViewSelectionChanged;
        itemTreeView.style.flexGrow = 1;

        // Populate TreeView with scriptable objects
        List<TreeViewItemData<string>> treeItems = LoadItemHierarchy();
        itemTreeView.SetRootItems(treeItems);
        tabContent.Add(itemTreeView);
    }

    private List<TreeViewItemData<string>> LoadItemHierarchy()
    {
        var items = UIAssetLoader.LoadAssetsByType<SO_Item>();
        var classes = UIAssetLoader.LoadAssetsByType<SO_Item_Class>();
        var types = UIAssetLoader.LoadAssetsByType<SO_Class_Type>();
        var effects = UIAssetLoader.LoadAssetsByType<SO_Item_Effect>();
        var triggers = UIAssetLoader.LoadAssetsByType<SO_Effect_Trigger>();

        List<TreeViewItemData<string>> treeItems = new List<TreeViewItemData<string>>();
        int idCounter = 0;

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

    private void ShowModuleListView(string moduleType)
    {
        itemTreeView.selectionChanged -= OnTreeViewSelectionChanged;
        itemTreeView = new TreeView();
        itemTreeView.selectionChanged += OnTreeViewSelectionChanged;
        itemTreeView.style.flexGrow = 1;

        List<TreeViewItemData<string>> treeItems = new List<TreeViewItemData<string>>();
        int idCounter = 0;

        if (moduleType == "Classes")
        {
            var classes = UIAssetLoader.LoadAssetsByType<SO_Item_Class>();
            var types = UIAssetLoader.LoadAssetsByType<SO_Class_Type>();

            foreach (var cls in classes)
            {
                List<TreeViewItemData<string>> classChildren = new List<TreeViewItemData<string>>();

                foreach (var type in types.Where(t => cls.Types.Contains(t)))
                {
                    classChildren.Add(new TreeViewItemData<string>(idCounter++, type.name));
                }

                treeItems.Add(new TreeViewItemData<string>(idCounter++, cls.name, classChildren));
            }
        }
        else if (moduleType == "Effects")
        {
            var effects = UIAssetLoader.LoadAssetsByType<SO_Item_Effect>();
            var triggers = UIAssetLoader.LoadAssetsByType<SO_Effect_Trigger>();

            foreach (var effect in effects)
            {
                List<TreeViewItemData<string>> effectChildren = new List<TreeViewItemData<string>>();

                foreach (var trigger in triggers.Where(t => effect.Trigger == t))
                {
                    effectChildren.Add(new TreeViewItemData<string>(idCounter++, trigger.name));
                }

                treeItems.Add(new TreeViewItemData<string>(idCounter++, effect.name, effectChildren));
            }
        }
        else if (moduleType == "Types")
        {
            var types = UIAssetLoader.LoadAssetsByType<SO_Class_Type>();
            foreach (var type in types)
            {
                treeItems.Add(new TreeViewItemData<string>(idCounter++, type.name));
            }
        }
        else if (moduleType == "Triggers")
        {
            var triggers = UIAssetLoader.LoadAssetsByType<SO_Effect_Trigger>();
            foreach (var trigger in triggers)
            {
                treeItems.Add(new TreeViewItemData<string>(idCounter++, trigger.name));
            }
        }

        itemTreeView.SetRootItems(treeItems);

        tabContent.Add(itemTreeView);
    }

    private void OnTreeViewSelectionChanged(IEnumerable<object> selectedItems)
    {
        foreach (var selectedItem in selectedItems)
        {
            ScriptableObject so = UIAssetLoader.LoadAssetByName<ScriptableObject>(selectedItem.ToString());
            inspectorPanel.Show(so);
        }
    }
}