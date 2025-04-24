using ItemSystem.MainModule;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Inspector panel for displaying and editing item module properties.
    /// </summary>
    public class InspectorPanel : VisualElement
    {
        public InspectorPanel()
        {
            style.flexGrow = 1;
            Add(new Label("Select an item to view details"));
        }

        public void Show(ScriptableObject _Obj, Action<bool> _InspectorValueChangeCallback)
        {
            Clear();
            if (_Obj == null)
            {
                Add(new Label("No item selected"));
                return;
            }

            Add(new Label($"Editing: {(_Obj as IItemModule).ModuleName}"));

            foreach (var property in _Obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                VisualElement entry = InspectorDataManager.CreateEntry(_Obj, property, this, _InspectorValueChangeCallback);
                if (entry != null)
                {
                    Add(entry);
                }
            }
        }
    }
}