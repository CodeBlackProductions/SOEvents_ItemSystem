using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class InspectorList<T> : VisualElement where T : ScriptableObject
{
    private VisualElement parentView = new VisualElement();
    private ListView listView;
    private List<T> items;

    private T[] originalArray;
    private Dictionary<string, T> originalDictionary;

    public Action<T> ItemAddCallback;
    public Action<T> ItemRemoveCallback;

    public InspectorList(List<T> sourceList, string title)
    {
        items = sourceList;
        InstantiateUI(title);
    }

    public InspectorList(T[] sourceArray, string title)
    {
        items = sourceArray.ToList();
        InstantiateUI(title);
    }

    public InspectorList(Dictionary<string, T> sourceDictionary, string title)
    {
        items = sourceDictionary.Values.ToList();

        InstantiateUI(title);
    }

    private void InstantiateUI(string title)
    {
        // Create title label
        Label titleLabel = new Label(title) { style = { unityFontStyleAndWeight = FontStyle.Bold } };
        parentView.Add(titleLabel);

        // Create ListView
        listView = new ListView(items, 20, CreateItem, BindItem);
        listView.selectionType = SelectionType.Single;
        listView.style.flexGrow = 1;

        // Buttons
        var buttonContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };

        Button addButton = new Button(() => ChooseNewItem()) { text = "Add" };
        Button removeButton = new Button(() => RemoveSelectedItem()) { text = "Remove" };

        buttonContainer.Add(addButton);
        buttonContainer.Add(removeButton);
        parentView.Add(buttonContainer);
        parentView.Add(listView);
        Add(parentView);
    }

    private VisualElement CreateItem() => new Label();

    private void BindItem(VisualElement element, int index)
    {
        (element as Label).text = items[index] != null ? items[index].name : "Null";
    }

    private void ChooseNewItem()
    {
        List<T> soList = UIAssetLoader.LoadAssetsByType<T>();
        List<string> soNames = new List<string>();
        soNames.Add("Choose new entry");
        for (int i = 0; i < soList.Count; i++)
        {
            soNames.Add(soList[i].name);
        }
        DropdownField dropdownField = new DropdownField(soNames, soNames[0]);
        dropdownField.RegisterValueChangedCallback(v => AddItem(v.newValue, dropdownField));
        parentView.Add(dropdownField);
    }

    private void AddItem(string _Item, DropdownField _SelectionDropdown)
    {
        _SelectionDropdown.RemoveFromHierarchy();

        T newItem = UIAssetLoader.LoadAssetByName<T>(_Item);
        items.Add(newItem);

        ItemAddCallback?.Invoke(newItem);

        listView.Rebuild();
    }

    private void RemoveSelectedItem()
    {
        if (listView.selectedItem == null) return;

        ItemRemoveCallback?.Invoke((T)listView.selectedItem);

        items.Remove((T)listView.selectedItem);

        listView.Rebuild();
    }
}