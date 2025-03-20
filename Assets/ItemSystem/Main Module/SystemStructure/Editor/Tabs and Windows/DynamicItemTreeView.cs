using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class DynamicItemTreeView<T> : VisualElement where T : ScriptableObject
{
    private int m_IDCounter = 0;
    private UnityEngine.UIElements.TreeView m_TreeView;
    private Button m_AddNewSOButton;

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
            m_AddNewSOButton = new Button(() => ModuleCreatorWindow.ShowWindow())
            {
                text = "Add",
                style =
                {
                    height = 25,
                    alignSelf = Align.Center
                }
            };

            hierarchy.Add(m_AddNewSOButton);
        }
    }

    public void RefreshTreeView()
    {
        m_IDCounter = 0;
        LoadHierarchy();
    }

    private void LoadHierarchy()
    {
        List<TreeViewItemData<string>> treeItems = LoadModules(typeof(T));
        m_TreeView.SetRootItems(treeItems);
        m_TreeView.Rebuild();
    }

    private List<TreeViewItemData<string>> LoadModules(Type _BaseType)
    {
        List<TreeViewItemData<string>> treeItems = new List<TreeViewItemData<string>>();

        var items = ItemEditor_AssetLoader.LoadAssetsByTypeReference(_BaseType);
        foreach (var item in items)
        {
            List<TreeViewItemData<string>> itemChildren = LoadSubModules(item);
            treeItems.Add(new TreeViewItemData<string>(GetNextId(), item.name, itemChildren));
        }

        return treeItems;
    }

    private List<TreeViewItemData<string>> LoadSubModules(ScriptableObject _Item)
    {
        List<TreeViewItemData<string>> itemChildren = new List<TreeViewItemData<string>>();

        var properties = _Item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (typeof(ScriptableObject).IsAssignableFrom(property.PropertyType))
            {
                var subItem = property.GetValue(_Item) as ScriptableObject;
                if (subItem != null)
                {
                    itemChildren.Add(new TreeViewItemData<string>(GetNextId(), subItem.name, LoadSubModules(subItem)));
                }
            }
            else if (typeof(IEnumerable<ScriptableObject>).IsAssignableFrom(property.PropertyType))
            {
                var subItems = property.GetValue(_Item) as IEnumerable<ScriptableObject>;
                if (subItems != null)
                {
                    foreach (var subItem in subItems)
                    {
                        itemChildren.Add(new TreeViewItemData<string>(GetNextId(), subItem.name, LoadSubModules(subItem)));
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
}