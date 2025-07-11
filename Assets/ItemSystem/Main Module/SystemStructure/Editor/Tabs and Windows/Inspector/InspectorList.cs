using ItemSystem.MainModule;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Custom list view for displaying and managing a list of ScriptableObject items.
    /// </summary>
    /// <typeparam name="T">Type of scriptable object allowed.</typeparam>
    public class InspectorList<T> : VisualElement where T : ScriptableObject
    {
        private VisualElement m_ParentView = new VisualElement();
        private ListView m_ListView;
        private List<T> m_Items;
        private List<System.Type> m_TypesToExclude = new List<System.Type>();

        public Action<T> ItemAddCallback;
        public Action<T> ItemRemoveCallback;
        public Action<T> ItemSelectCallback;

        public InspectorList(List<T> _SourceList, List<System.Type> _TypesToExclude, string _Title, bool _ShowAddAndRemove)
        {
            if (_SourceList == null)
            {
                m_Items = new List<T>();
            }
            else
            {
                m_Items = _SourceList;
            }

            if (_TypesToExclude != null && _TypesToExclude.Count > 0)
            {
                m_TypesToExclude = _TypesToExclude;
            }

            InstantiateUI(_Title, _ShowAddAndRemove);
        }

        public InspectorList(T[] _SourceArray, List<System.Type> _TypesToExclude, string _Title, bool _ShowAddAndRemove)
        {
            if (_SourceArray == null)
            {
                m_Items = new List<T>();
            }
            else
            {
                m_Items = _SourceArray.ToList();
            }

            if (_TypesToExclude != null && _TypesToExclude.Count > 0)
            {
                m_TypesToExclude = _TypesToExclude;
            }

            InstantiateUI(_Title, _ShowAddAndRemove);
        }

        public InspectorList(Dictionary<string, T> _SourceDictionary, List<System.Type> _TypesToExclude, string _Title, bool _ShowAddAndRemove)
        {
            if (_SourceDictionary == null)
            {
                m_Items = new List<T>();
            }
            else
            {
                m_Items = _SourceDictionary.Values.ToList();
            }

            if (_TypesToExclude != null && _TypesToExclude.Count > 0)
            {
                m_TypesToExclude = _TypesToExclude;
            }

            InstantiateUI(_Title, _ShowAddAndRemove);
        }

        private void InstantiateUI(string _Title, bool _ShowAddAndRemove)
        {
            Label titleLabel = new Label(_Title) { style = { unityFontStyleAndWeight = FontStyle.Bold } };
            m_ParentView.Add(titleLabel);

            m_ListView = new ListView(m_Items, 20, CreateItem, BindItem);
            m_ListView.selectionType = SelectionType.Single;
            m_ListView.style.flexGrow = 1;

            m_ListView.selectionChanged += (s) => ItemSelectCallback?.Invoke((T)m_ListView.selectedItem);

            var buttonContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            Button addButton = new Button(() => ChooseNewItem()) { text = "Add", style = { minHeight = 20 } };
            Button removeButton = new Button(() => RemoveSelectedItem()) { text = "Remove", style = { minHeight = 20 } };

            buttonContainer.Add(addButton);
            buttonContainer.Add(removeButton);
            m_ParentView.Add(m_ListView);

            if (_ShowAddAndRemove)
            {
                m_ParentView.Add(buttonContainer);
            }
            Add(m_ParentView);
        }

        private VisualElement CreateItem() => new Label();

        private void BindItem(VisualElement _Element, int _Index)
        {
            (_Element as Label).text = m_Items[_Index] != null ? (m_Items[_Index] as IItemModule).ModuleName : "Null";
        }

        private void ChooseNewItem()
        {
            List<T> soList = ItemEditor_AssetLoader.LoadAssetsByType<T>();
            List<string> soNames = new List<string>();
            soNames.Add("Choose new entry");
            for (int i = 0; i < soList.Count; i++)
            {
                if (m_TypesToExclude != null)
                {
                    bool isExcluded = false;
                    for (int t = 0; t < m_TypesToExclude.Count; t++)
                    {
                        if (soList[i].GetType() == m_TypesToExclude[t])
                        {
                            isExcluded = true;
                            break;
                        }
                    }
                    if (isExcluded) continue;
                }

                if (soList[i] is not IItemModule || (soList[i] as IItemModule).ModuleName == "EditorSettings" || soList[i].GetType().IsAbstract)
                {
                    continue;
                }
                soNames.Add((soList[i] as IItemModule).ModuleName);
            }
            DropdownField dropdownField = new DropdownField(soNames, soNames[0]);
            dropdownField.RegisterValueChangedCallback(v => AddItem(v.newValue, dropdownField));
            m_ParentView.Add(dropdownField);
        }

        private void AddItem(string _Item, DropdownField _SelectionDropdown)
        {
            _SelectionDropdown.RemoveFromHierarchy();

            T newItem = ItemEditor_AssetLoader.LoadAssetsByType<T>().FirstOrDefault(so => (so as IItemModule).ModuleName == _Item);

            if (newItem != null)
            {
                m_Items.Add(newItem);

                ItemAddCallback?.Invoke(newItem);

                m_ListView.Rebuild();
            }
        }

        private void RemoveSelectedItem()
        {
            if (m_ListView.selectedItem == null) return;

            ItemRemoveCallback?.Invoke((T)m_ListView.selectedItem);

            m_Items.Remove((T)m_ListView.selectedItem);

            m_ListView.Rebuild();
        }
    }
}