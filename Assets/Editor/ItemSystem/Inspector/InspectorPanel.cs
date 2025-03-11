using static UnityEngine.Rendering.DebugUI.MessageBox;
using System.Reflection;
using UnityEngine.UIElements;
using UnityEngine;

public class InspectorPanel : VisualElement
{
    public InspectorPanel()
    {
        style.flexGrow = 1;
        Add(new Label("Select an item to view details"));
    }

    public void Show(ScriptableObject obj)
    {
        Clear();
        if (obj == null)
        {
            Add(new Label("No item selected"));
            return;
        }

        Add(new Label($"Editing: {obj.name}"));

        foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            VisualElement entry = InspectorDataManager.CreateEntry(obj, property);
            if (entry != null)
            {
                Add(entry);
            }
        }
    }
}