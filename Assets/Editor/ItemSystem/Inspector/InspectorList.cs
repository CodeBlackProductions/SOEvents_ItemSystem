using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InspectorList<T> : VisualElement where T : ScriptableObject
{
    private VisualElement parentView = new VisualElement();
    private ListView listView;
    private List<T> items;
    private Action<T> onItemSelected;

    public InspectorList(List<T> sourceList, string title)
    {
        items = sourceList;
        onItemSelected = OnListSelectionChange;

        // Create title label
        Label titleLabel = new Label(title) { style = { unityFontStyleAndWeight = FontStyle.Bold } };
        parentView.Add(titleLabel);

        // Create ListView
        listView = new ListView(items, 20, CreateItem, BindItem);
        listView.selectionType = SelectionType.Single;
        listView.style.flexGrow = 1;
        listView.selectionChanged += selection => onItemSelected?.Invoke(listView.selectedItem as T);

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
        listView.Rebuild();
    }

    private void RemoveSelectedItem()
    {
        if (listView.selectedItem == null) return;
        items.Remove((T)listView.selectedItem);
        listView.Rebuild();
    }

    private void OnListSelectionChange(T _Selection)
    {
        Debug.Log($"You selected something on an Inspector-List: {_Selection?.name}");
    }
}