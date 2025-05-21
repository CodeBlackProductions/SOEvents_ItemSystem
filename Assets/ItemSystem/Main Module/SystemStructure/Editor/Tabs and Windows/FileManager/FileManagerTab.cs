using ItemSystem.MainModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Tab in the Item System editor that allows users to Load from or write to JSON files.
    /// </summary>
    public class FileManagerTab : TabBase
    {
        public FileManagerTab(Action<bool> _InspectorValueChangeCallback, Action<IEnumerable<System.Object>, bool, bool> _TreeviewSelectionChangeCallback, Action<IEnumerable<System.Object>> _LocalFileTreeviewSelectionChangeCallback)
        {
            m_Root = new VisualElement();

            m_Root.style.flexDirection = FlexDirection.Column;
            m_Root.style.flexGrow = 1;

            LoadHierarchy();

            Add(m_Root);
        }

        protected override void OnSubTabChanged(Type _ModuleType, bool _ShowAddAndRemove, bool _LoadSubTypes, bool _ShowInspectorPanel, bool _LoadLocalFiles)
        {
            if (m_FilterPanel != null)
            {
                m_FilterPanel.ClearFilter();
            }

            m_SubTabContent.Clear();
            MethodInfo method = typeof(FileManagerTab).GetMethod("LoadSubTabHierarchy", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo generic = method.MakeGenericMethod(_ModuleType);
            generic.Invoke(this, new object[] { _ShowAddAndRemove, _ShowInspectorPanel, _LoadSubTypes, _LoadLocalFiles });
        }

        protected override void LoadHierarchy()
        {
            m_SubTabContent?.Clear();

            Type[] baseTypes = ItemEditor_AssetLoader.LoadAllBaseTypes().ToArray();
            KeyValuePair<string, Type>[] statTypePairs = new KeyValuePair<string, Type>[baseTypes.Length];

            for (int i = 0; i < statTypePairs.Length; i++)
            {
                statTypePairs[i] = new KeyValuePair<string, Type>(baseTypes[i].Name.Substring(baseTypes[i].Name.LastIndexOf("_") + 1), baseTypes[i]);
            }

            m_SubTabMenu = new TabbedMenu(statTypePairs, OnSubTabChanged, false, false, false, true);
            m_SubTabContent = new VisualElement();

            m_SubTabContent.style.flexDirection = FlexDirection.Row;
            m_SubTabContent.style.flexGrow = 1;

            m_Root.Add(m_SubTabMenu);

            LoadFilterPanel();
            LoadSubTabHierarchy<SO_Item>(false, false, false, true);
            OnSubTabChanged(typeof(SO_Item), false, false, false, true);

            m_Root.Add(m_SubTabContent);
        }
    }
}