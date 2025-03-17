using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class InspectorPanel : VisualElement
{
    public InspectorPanel()
    {
        style.flexGrow = 1;
        Add(new Label("Select an item to view details"));
    }

    public void Show(ScriptableObject _Obj)
    {
        Clear();
        if (_Obj == null)
        {
            Add(new Label("No item selected"));
            return;
        }

        Add(new Label($"Editing: {_Obj.name}"));

        foreach (var property in _Obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            VisualElement entry = InspectorDataManager.CreateEntry(_Obj, property, this);
            if (entry != null)
            {
                Add(entry);
            }
        }
    }
}