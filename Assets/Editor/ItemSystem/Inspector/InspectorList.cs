using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class InspectorList<T> : VisualElement where T : ScriptableObject
{
    private ListView listView;
    private List<T> items;
    private Action<T> onItemSelected;

    public InspectorList(List<T> sourceList, string title, Action<T> onSelect)
    {
        items = sourceList;
        onItemSelected = onSelect;

        // Create title label
        Label titleLabel = new Label(title) { style = { unityFontStyleAndWeight = FontStyle.Bold } };
        Add(titleLabel);

        // Create ListView
        listView = new ListView(items, 20, CreateItem, BindItem);
        listView.selectionType = SelectionType.Single;
        listView.style.flexGrow = 1;
        listView.selectionChanged += selection => onItemSelected?.Invoke((T)selection.Take(1));

        // Buttons
        var buttonContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };

        Button addButton = new Button(() => AddItem()) { text = "Add" };
        Button removeButton = new Button(() => RemoveSelectedItem()) { text = "Remove" };

        buttonContainer.Add(addButton);
        buttonContainer.Add(removeButton);
        Add(buttonContainer);
        Add(listView);
    }

    private VisualElement CreateItem() => new Label();

    private void BindItem(VisualElement element, int index)
    {
        (element as Label).text = items[index] != null ? items[index].name : "Null";
    }

    private void AddItem()
    {
        T newItem = ScriptableObject.CreateInstance<T>();
        newItem.name = $"New {typeof(T).Name}";
        items.Add(newItem);
        listView.Rebuild();
    }

    private void RemoveSelectedItem()
    {
        if (listView.selectedItem == null) return;
        items.Remove((T)listView.selectedItem);
        listView.Rebuild();
    }
}