using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class InspectorList<T> : VisualElement where T : ScriptableObject
{
    private VisualElement m_ParentView = new VisualElement();
    private ListView m_ListView;
    private List<T> m_Items;

    public Action<T> ItemAddCallback;
    public Action<T> ItemRemoveCallback;

    public InspectorList(List<T> _SourceList, string _Title)
    {
        if (_SourceList == null)
        {
            m_Items = new List<T>();
        }
        else
        {
            m_Items = _SourceList;
        }

        InstantiateUI(_Title);
    }

    public InspectorList(T[] _SourceArray, string _Title)
    {
        if (_SourceArray == null)
        {
            m_Items = new List<T>();
        }
        else
        {
            m_Items = _SourceArray.ToList();
        }

        InstantiateUI(_Title);
    }

    public InspectorList(Dictionary<string, T> _SourceDictionary, string _Title)
    {
        if (_SourceDictionary == null)
        {
            m_Items = new List<T>();
        }
        else
        {
            m_Items = _SourceDictionary.Values.ToList();
        }

        InstantiateUI(_Title);
    }

    private void InstantiateUI(string _Title)
    {
        // Create title label
        Label titleLabel = new Label(_Title) { style = { unityFontStyleAndWeight = FontStyle.Bold } };
        m_ParentView.Add(titleLabel);

        // Create ListView
        m_ListView = new ListView(m_Items, 20, CreateItem, BindItem);
        m_ListView.selectionType = SelectionType.Single;
        m_ListView.style.flexGrow = 1;

        // Buttons
        var buttonContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };

        Button addButton = new Button(() => ChooseNewItem()) { text = "Add" };
        Button removeButton = new Button(() => RemoveSelectedItem()) { text = "Remove" };

        buttonContainer.Add(addButton);
        buttonContainer.Add(removeButton);
        m_ParentView.Add(buttonContainer);
        m_ParentView.Add(m_ListView);
        Add(m_ParentView);
    }

    private VisualElement CreateItem() => new Label();

    private void BindItem(VisualElement _Element, int _Index)
    {
        (_Element as Label).text = m_Items[_Index] != null ? m_Items[_Index].name : "Null";
    }

    private void ChooseNewItem()
    {
        List<T> soList = ItemEditor_AssetLoader.LoadAssetsByType<T>();
        List<string> soNames = new List<string>();
        soNames.Add("Choose new entry");
        for (int i = 0; i < soList.Count; i++)
        {
            soNames.Add(soList[i].name);
        }
        DropdownField dropdownField = new DropdownField(soNames, soNames[0]);
        dropdownField.RegisterValueChangedCallback(v => AddItem(v.newValue, dropdownField));
        m_ParentView.Add(dropdownField);
    }

    private void AddItem(string _Item, DropdownField _SelectionDropdown)
    {
        _SelectionDropdown.RemoveFromHierarchy();

        T newItem = ItemEditor_AssetLoader.LoadAssetByName<T>(_Item);
        m_Items.Add(newItem);

        ItemAddCallback?.Invoke(newItem);

        m_ListView.Rebuild();
    }

    private void RemoveSelectedItem()
    {
        if (m_ListView.selectedItem == null) return;

        ItemRemoveCallback?.Invoke((T)m_ListView.selectedItem);

        m_Items.Remove((T)m_ListView.selectedItem);

        m_ListView.Rebuild();
    }
}