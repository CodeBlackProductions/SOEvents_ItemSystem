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
        int m_MaintabColor = -1;

        public InspectorPanel(int _MainTabColor)
        {
            style.flexGrow = 1;
            style.paddingTop = 10;
            style.paddingLeft = 5;
            m_MaintabColor = _MainTabColor;

            Label title = new Label("Select an item to view details");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 15;
            Add(title);
        }

        public void Show(ScriptableObject _Obj, Action<bool> _InspectorValueChangeCallback)
        {
            Clear();
            if (_Obj == null)
            {
                Add(new Label("No item selected"));
                return;
            }

            Label title = new Label($"Editing: {(_Obj as IItemModule).ModuleName}");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 15;
            title.style.paddingBottom = 10;
            Add(title);

            foreach (var property in _Obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                VisualElement entry = InspectorDataManager.CreateEntry(_Obj, property, this, _InspectorValueChangeCallback, m_MaintabColor);
                if (entry != null)
                {
                    Add(entry);
                }
            }
        }
    }
}