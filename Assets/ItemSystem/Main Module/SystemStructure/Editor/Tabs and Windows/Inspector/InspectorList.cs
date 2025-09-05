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

        private string m_selectedToAdd = "";
        private DropdownField m_DropdownField;

        public Action<T> ItemAddCallback;
        public Action<T> ItemRemoveCallback;
        public Action<T> ItemSelectCallback;

        public InspectorList(
            List<T> _SourceList,
            List<System.Type> _TypesToExclude,
            string _Title,
            bool _ShowAddAndRemove,
            int _ButtonColor = -1,
            bool _CompactSize = false,
            bool _HideTitle = false
            )
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

            InstantiateUI(_Title, _ShowAddAndRemove, _ButtonColor, _CompactSize, _HideTitle);
        }

        public InspectorList(
            T[] _SourceArray,
            List<System.Type> _TypesToExclude,
            string _Title,
            bool _ShowAddAndRemove,
            int _ButtonColor = -1,
            bool _CompactSize = false,
            bool _HideTitle = false
            )
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

            InstantiateUI(_Title, _ShowAddAndRemove, _ButtonColor, _CompactSize, _HideTitle);
        }

        public InspectorList(
            Dictionary<string, T> _SourceDictionary,
            List<System.Type> _TypesToExclude,
            string _Title,
            bool _ShowAddAndRemove,
            int _ButtonColor = -1,
            bool _CompactSize = false,
            bool _HideTitle = false
            )
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

            InstantiateUI(_Title, _ShowAddAndRemove, _ButtonColor, _CompactSize, _HideTitle);
        }

        private void InstantiateUI(string _Title, bool _ShowAddAndRemove, int _ButtonColor = -1, bool _CompactSize = false, bool _HideTitle = false)
        {
            if (!_HideTitle)
            {
                Label title = new Label(_Title);
                title.style.unityFontStyleAndWeight = FontStyle.Bold;
                title.style.minWidth = 100;
                title.style.marginLeft = 2;
                m_ParentView.Add(title);
            }

            m_ListView = new ListView(m_Items, 20, CreateItem, BindItem);
            m_ListView.selectionType = SelectionType.Single;
            m_ListView.style.flexGrow = 1;
            m_ListView.style.minHeight = 20;

            if (_CompactSize)
            {
                m_ListView.style.maxHeight = 60;
            }

            m_ListView.selectionChanged += (s) => ItemSelectCallback?.Invoke((T)m_ListView.selectedItem);

            StyleSheet styleSheet = UI_Styles_Lib.GetUIStyles();

            m_DropdownField = InstantiateDropDown(styleSheet, _ButtonColor, _CompactSize);

            Button addButton = new Button(() => AddSelectedItem(m_selectedToAdd)) { text = "Add", style = { minHeight = 20, minWidth = 60 } };
            addButton.AddToClassList($"tab-c-{_ButtonColor}");

            Button removeButton = new Button(() => RemoveSelectedItem()) { text = "Remove", style = { minHeight = 20 } };
            removeButton.AddToClassList($"tab-c-{_ButtonColor}");

            var selectionContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            selectionContainer.styleSheets.Add(styleSheet);

            if (!_CompactSize)
            {
                Label newElementLabel = new Label("Select new:");
                newElementLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                newElementLabel.style.minWidth = 100;
                newElementLabel.style.marginLeft = 2;
                selectionContainer.Add(newElementLabel);
            }

            selectionContainer.Add(m_DropdownField);
            selectionContainer.Add(addButton);
            selectionContainer.style.minHeight = 20;
            selectionContainer.style.paddingBottom = 2;
            selectionContainer.style.borderBottomColor = Color.gray;
            selectionContainer.style.borderBottomWidth = 1;

            if (_ShowAddAndRemove)
            {
                m_ParentView.Add(selectionContainer);
            }

            m_ParentView.Add(m_ListView);
            m_ParentView.Add(removeButton);

            Add(m_ParentView);
        }

        private VisualElement CreateItem() => new Label();

        private void BindItem(VisualElement _Element, int _Index)
        {
            for (int i = m_DropdownField.choices.Count - 1; i >= 0; i--)
            {
                if ((m_Items[_Index] as IItemModule).ModuleName == m_DropdownField.choices[i])
                {
                    m_DropdownField.choices.RemoveAt(i);
                }
            }

            (_Element as Label).text = m_Items[_Index] != null ? (m_Items[_Index] as IItemModule).ModuleName : "Null";
        }

        private DropdownField InstantiateDropDown(StyleSheet _StyleSheet, int _ButtonColor = -1, bool _CompactSize = false)
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
            dropdownField.RegisterValueChangedCallback(v => { m_selectedToAdd = v.newValue; });

            dropdownField.styleSheets.Add(_StyleSheet);

            VisualElement ve = dropdownField;

            if (_CompactSize)
            {
                ve.ElementAt(0).AddToClassList("inspector-dropdown-compact");
            }
            else
            {
                ve.ElementAt(0).AddToClassList("inspector-dropdown");
            }

            if (_ButtonColor != -1)
            {
                ve.ElementAt(0).AddToClassList($"tab-c-{_ButtonColor}");
            }
            else
            {
                ve.ElementAt(0).AddToClassList("tab-c-default");
            }

            return dropdownField;
        }

        private void AddSelectedItem(string _Item)
        {
            T newItem = ItemEditor_AssetLoader.LoadAssetsByType<T>().FirstOrDefault(so => (so as IItemModule)?.ModuleName == _Item);

            if (newItem != null)
            {
                m_Items.Add(newItem);

                ItemAddCallback?.Invoke(newItem);

                m_ListView.Rebuild();
            }

            m_DropdownField.index = 0;
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