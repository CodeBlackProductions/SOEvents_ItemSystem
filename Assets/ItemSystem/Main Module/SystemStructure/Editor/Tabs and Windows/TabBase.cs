using ItemSystem.MainModule;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Base class for all tabs in the Item System editor.
    /// </summary>
    public abstract class TabBase : VisualElement
    {
        protected VisualElement m_Root;
        protected TabbedMenu m_SubTabMenu;
        protected FilterPanel m_FilterPanel;
        protected VisualElement m_SubTabContent;
        protected InspectorPanel m_SubTabInspectorPanel;

        protected Action<bool> m_InspectorValueChangeCallback;
        protected Action<IEnumerable<System.Object>, bool, bool> m_TreeviewSelectionChangeCallback;
        protected Action<IEnumerable<System.Object>> m_LocalFileTreeviewSelectionChangeCallback;

        protected List<ScriptableObject> m_LocalFileItems = new List<ScriptableObject>();
        protected Button m_BTN_SaveToFile;
        protected Button m_BTN_LoadIntoAssets;

        protected abstract void OnSubTabChanged(System.Type _ModuleType, bool _ShowAddAndRemove, bool _LoadSubTypes, bool _ShowInspectorPanel, bool _LoadLocalFiles, int _ButtonColor);

        protected abstract void LoadHierarchy(int _ButtonColor);

        protected void SetTabBackgroundColor(int _Color)
        {
            Button temp = new Button();
            temp.AddToClassList("tab-button");
            temp.AddToClassList($"tab-c{_Color}");
            Color btnCol = temp.resolvedStyle.backgroundColor;

            Color darkenedColor = btnCol;
            darkenedColor *= 0.35f;
            darkenedColor.a = 0.35f;

            if (m_FilterPanel != null)
            {
                m_FilterPanel.style.backgroundColor = new StyleColor(darkenedColor);
            }
            if (m_SubTabMenu != null)
            {
                m_SubTabMenu.style.backgroundColor = new StyleColor(darkenedColor);
            }
            if (m_SubTabContent != null) 
            {
                m_SubTabContent.style.backgroundColor = new StyleColor(darkenedColor);
            }
            if (m_SubTabInspectorPanel != null)
            {
                m_SubTabInspectorPanel.style.backgroundColor = new StyleColor(darkenedColor);
            }
        }

        protected void LoadFilterPanel(System.Type _ObjectType, int _ButtonColor)
        {
            m_FilterPanel = new FilterPanel(_ObjectType, _ButtonColor);

            m_Root.Add(m_FilterPanel);
        }

        protected void LoadSubTabHierarchy<T>(bool _ShowAddAndRemove, bool _ShowInspectorPanel, bool _LoadSubtypes, bool _LoadLocalFiles, int _ButtonColor) where T : ScriptableObject
        {
            if (m_FilterPanel != null)
            {
                m_FilterPanel.ClearFilter();
                m_FilterPanel.ChangeSortModeType(typeof(T));
            }

            m_TreeviewSelectionChangeCallback += OnTreeViewSelectionChanged;

            CreateTreeview<T>(_ShowAddAndRemove, _LoadSubtypes, _ShowInspectorPanel, _LoadLocalFiles, _ButtonColor);

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

        protected void CreateTreeview<T>(bool _ShowAddAndRemove, bool _LoadSubTypes, bool _ShowInspectorPanel, bool _ShowSaveToFile, int _ButtonColor) where T : ScriptableObject
        {
            DynamicItemTreeView<T> treeView = new DynamicItemTreeView<T>(m_TreeviewSelectionChangeCallback, _ShowAddAndRemove, _LoadSubTypes, _ShowInspectorPanel, _ShowSaveToFile, _ButtonColor);
            m_InspectorValueChangeCallback += treeView.RefreshTreeView;
            if (m_FilterPanel != null)
            {
                m_FilterPanel.OnFilterChangedCallback += treeView.SetTreeviewFilter;
                m_FilterPanel.OnSortModeChangedCallback += treeView.SetTreeviewSortMode;
            }

            treeView.style.flexGrow = 1;
            treeView.style.borderRightWidth = 2;
            treeView.style.borderRightColor = new StyleColor(Color.grey);
            treeView.style.marginRight = 5;

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