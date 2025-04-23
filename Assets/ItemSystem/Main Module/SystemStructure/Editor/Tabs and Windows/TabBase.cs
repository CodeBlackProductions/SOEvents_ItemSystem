using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    public abstract class TabBase : VisualElement
    {
        protected VisualElement m_Root;
        protected TabbedMenu m_SubTabMenu;
        protected VisualElement m_SubTabContent;
        protected InspectorPanel m_SubTabInspectorPanel;

        protected Action<bool> m_InspectorValueChangeCallback;
        protected Action<IEnumerable<System.Object>, bool, bool> m_TreeviewSelectionChangeCallback;
        protected Action<IEnumerable<System.Object>> m_LocalFileTreeviewSelectionChangeCallback;

        protected List<ScriptableObject> m_LocalFileItems = new List<ScriptableObject>();
        protected Button m_BTN_SaveToFile;
        protected Button m_BTN_LoadIntoAssets;

        protected abstract void OnSubTabChanged(Type _ModuleType, bool _ShowAddAndRemove, bool _LoadSubTypes, bool _ShowInspectorPanel, bool _LoadLocalFiles);

        protected abstract void LoadHierarchy();

        protected void LoadSubTabHierarchy<T>(bool _ShowAddAndRemove, bool _ShowInspectorPanel, bool _LoadSubtypes, bool _LoadLocalFiles) where T : ScriptableObject
        {
            CreateTreeview<T>(_ShowAddAndRemove, _LoadSubtypes, _ShowInspectorPanel, _LoadLocalFiles);
            m_TreeviewSelectionChangeCallback += OnTreeViewSelectionChanged;

            if (_LoadLocalFiles)
            {
                m_LocalFileItems = ItemEditor_FileManager.LoadModulesFromFiles(typeof(T));
                CreateTreeview<ScriptableObject>(m_LocalFileItems, false);
                m_LocalFileTreeviewSelectionChangeCallback += OnLocalFileTreeviewSelectionChanged;
            }

            if (_ShowInspectorPanel)
            {
                m_SubTabContent.Add(m_SubTabInspectorPanel);
            }
        }

        protected void CreateTreeview<T>(bool _ShowAddAndRemove, bool _LoadSubTypes, bool _ShowInspectorPanel, bool _ShowSaveToFile) where T : ScriptableObject
        {
            DynamicItemTreeView<T> treeView = new DynamicItemTreeView<T>(m_TreeviewSelectionChangeCallback, _ShowAddAndRemove, _LoadSubTypes, _ShowInspectorPanel, _ShowSaveToFile);
            m_InspectorValueChangeCallback += treeView.RefreshTreeView;
            treeView.style.flexGrow = 1;
            m_SubTabContent.Add(treeView);
        }

        protected void CreateTreeview<T>(List<ScriptableObject> _Items, bool _SaveToFile) where T : ScriptableObject
        {
            DynamicItemTreeView<T> treeView = new DynamicItemTreeView<T>(_Items, m_LocalFileTreeviewSelectionChangeCallback);
            m_InspectorValueChangeCallback += treeView.RefreshTreeView;
            treeView.style.flexGrow = 1;
            m_SubTabContent.Add(treeView);
        }

        protected void OnTreeViewSelectionChanged(IEnumerable<object> _SelectedItems, bool _ShowInspectorPanel, bool _ShowSaveToFile)
        {
            if (m_Root.Contains(m_BTN_SaveToFile))
            {
                m_Root.Remove(m_BTN_SaveToFile);
            }
            if (m_Root.Contains(m_BTN_LoadIntoAssets))
            {
                m_Root.Remove(m_BTN_LoadIntoAssets);
            }

            foreach (var selectedItem in _SelectedItems)
            {
                if (selectedItem is TreeViewEntryData data)
                {
                    if (_ShowSaveToFile)
                    {
                        ScriptableObject so = ItemEditor_AssetLoader.LoadAssetByName<ScriptableObject>(data.FileName);

                        m_BTN_SaveToFile = new Button(() =>
                        {
                            ItemEditor_FileManager.SaveModuleToFile(so);
                        });
                        m_BTN_SaveToFile.text = $"Save {(so as IItemModule).ModuleName} to JSON";

                        m_Root.Add(m_BTN_SaveToFile);
                    }

                    if (_ShowInspectorPanel)
                    {
                        ShowInspectorPanel(data.FileName, m_InspectorValueChangeCallback);
                    }
                }
            }
        }

        protected void OnLocalFileTreeviewSelectionChanged(IEnumerable<object> _SelectedItems)
        {
            if (m_Root.Contains(m_BTN_SaveToFile))
            {
                m_Root.Remove(m_BTN_SaveToFile);
            }
            if (m_Root.Contains(m_BTN_LoadIntoAssets))
            {
                m_Root.Remove(m_BTN_LoadIntoAssets);
            }

            foreach (var selectedItem in _SelectedItems)
            {
                if (selectedItem is TreeViewEntryData data)
                {
                    ScriptableObject localFileItem = m_LocalFileItems.FirstOrDefault(item => item.name == data.FileName);
                    if (localFileItem != null)
                    {
                        m_BTN_LoadIntoAssets = new Button(() =>
                        {
                            ItemEditor_InstanceManager.CreateInstance(localFileItem, localFileItem.GetType());
                        });
                        m_BTN_LoadIntoAssets.text = $"Load {(localFileItem as IItemModule).ModuleName} into Assetfolder";

                        m_Root.Add(m_BTN_LoadIntoAssets);
                    }
                }
            }
        }

        protected void ShowInspectorPanel(string _FileName, Action<bool> _InspectorValueChangeCallback)
        {
            ScriptableObject so = ItemEditor_AssetLoader.LoadAssetByName<ScriptableObject>(_FileName);
            m_SubTabInspectorPanel.Show(so, _InspectorValueChangeCallback);
        }
    }
}