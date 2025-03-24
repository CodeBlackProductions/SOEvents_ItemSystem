using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class DynamicItemTreeView<T> : VisualElement where T : ScriptableObject
{
    private int m_IDCounter = 0;
    private UnityEngine.UIElements.TreeView m_TreeView;
    private VisualElement m_ButtonPanel;
    private Button m_BTN_AddNewSO;
    private Button m_BTN_RemoveSelectedModule;

    public DynamicItemTreeView(Action<IEnumerable<System.Object>> _OnSelectionChangedCallback, bool _ShowAddAndRemove)
    {
        m_TreeView = new UnityEngine.UIElements.TreeView();
        m_TreeView.selectionChanged += (s) => _OnSelectionChangedCallback?.Invoke(s);
        LoadHierarchy();

        m_TreeView.style.flexGrow = 1;
        m_TreeView.style.alignSelf = Align.Stretch;

        this.style.flexDirection = FlexDirection.Column;
        this.style.flexGrow = 1;
        this.style.paddingRight = 50;

        hierarchy.Add(m_TreeView);

        if (_ShowAddAndRemove)
        {
            m_ButtonPanel = new VisualElement();
            m_ButtonPanel.style.flexDirection = FlexDirection.Row;
            m_ButtonPanel.style.alignSelf = Align.Center;
            m_ButtonPanel.style.flexGrow = 0;

            m_BTN_AddNewSO = new Button(() => ModuleCreatorWindow.ShowWindow(RefreshTreeView))
            {
                text = "Add",
                style =
                {
                    height = 25,
                    alignSelf = Align.Center
                }
            };

            m_ButtonPanel.Add(m_BTN_AddNewSO);

            m_BTN_RemoveSelectedModule = new Button(() => RemoveSelectedModule())
            {
                text = "Remove",
                style =
                {
                    height = 25,
                    alignSelf = Align.Center
                }
            };

            m_ButtonPanel.Add(m_BTN_RemoveSelectedModule);

            hierarchy.Add(m_ButtonPanel);
        }
    }

    public void RefreshTreeView()
    {
        m_IDCounter = 0;
        LoadHierarchy();
    }

    private void LoadHierarchy()
    {
        List<TreeViewItemData<TreeViewEntryData>> treeItems = LoadModules(typeof(T));
        m_TreeView.SetRootItems(treeItems);
        m_TreeView.Rebuild();
    }

    private List<TreeViewItemData<TreeViewEntryData>> LoadModules(Type _BaseType)
    {
        List<TreeViewItemData<TreeViewEntryData>> treeItems = new List<TreeViewItemData<TreeViewEntryData>>();

        var items = ItemEditor_AssetLoader.LoadAssetsByTypeReference(_BaseType);
        foreach (var item in items)
        {
            List<TreeViewItemData<TreeViewEntryData>> itemChildren = LoadSubModules(item);

            string fileName = item.name;
            string itemName = (item as IItemModule).ModuleName;
            treeItems.Add(new TreeViewItemData<TreeViewEntryData>(GetNextId(), new TreeViewEntryData(itemName, fileName), itemChildren));
        }

        return treeItems;
    }

    private List<TreeViewItemData<TreeViewEntryData>> LoadSubModules(ScriptableObject _Item)
    {
        List<TreeViewItemData<TreeViewEntryData>> itemChildren = new List<TreeViewItemData<TreeViewEntryData>>();

        var properties = _Item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (typeof(ScriptableObject).IsAssignableFrom(property.PropertyType))
            {
                var subItem = property.GetValue(_Item) as ScriptableObject;
                if (subItem != null)
                {
                    string fileName = subItem.name;
                    string itemName = (subItem as IItemModule).ModuleName;
                    itemChildren.Add(new TreeViewItemData<TreeViewEntryData>(GetNextId(), new TreeViewEntryData(itemName, fileName), LoadSubModules(subItem)));
                }
            }
            else if (typeof(IEnumerable<ScriptableObject>).IsAssignableFrom(property.PropertyType))
            {
                var subItems = property.GetValue(_Item) as IEnumerable<ScriptableObject>;
                if (subItems != null)
                {
                    foreach (var subItem in subItems)
                    {
                        if (subItem != null)
                        {
                            string fileName = subItem.name;
                            string itemName = (subItem as IItemModule).ModuleName;
                            itemChildren.Add(new TreeViewItemData<TreeViewEntryData>(GetNextId(), new TreeViewEntryData(itemName, fileName), LoadSubModules(subItem)));
                        }
                    }
                }
            }
        }

        return itemChildren;
    }

    private int GetNextId()
    {
        return m_IDCounter++;
    }

    private void RemoveSelectedModule()
    {
        object selection = m_TreeView.selectedItem;

        if (selection != null)
        {
            ScriptableObject deleteFile = ItemEditor_AssetLoader.LoadAssetByName<ScriptableObject>((selection as TreeViewEntryData).FileName);
            ItemEditor_InstanceManager.RemoveInstance(deleteFile);
        }

        RefreshTreeView();
    }
}

public class TreeViewEntryData
{
    public string DisplayName { get; }
    public string FileName { get; }

    public TreeViewEntryData(string _DisplayName, string _FileName)
    {
        DisplayName = _DisplayName;
        FileName = _FileName;
    }

    public override string ToString()
    {
        return DisplayName;
    }
}