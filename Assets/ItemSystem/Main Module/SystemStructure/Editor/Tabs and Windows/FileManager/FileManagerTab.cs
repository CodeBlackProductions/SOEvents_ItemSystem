using ItemSystem.MainModule;
using ItemSystem.SubModules;
using System;
using System.Collections.Generic;
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
            m_SubTabContent.Clear();
            MethodInfo method = typeof(FileManagerTab).GetMethod("LoadSubTabHierarchy", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo generic = method.MakeGenericMethod(_ModuleType);
            generic.Invoke(this, new object[] { _ShowAddAndRemove, _ShowInspectorPanel,_LoadSubTypes, _LoadLocalFiles });
        }

        protected override void LoadHierarchy()
        {
            m_SubTabContent?.Clear();
            m_SubTabMenu = new TabbedMenu(new KeyValuePair<string, Type>[]
           {
          new KeyValuePair<string, Type>("Items", typeof(SO_Item)),
          new KeyValuePair<string, Type>("Classes", typeof(SO_Item_Class)),
          new KeyValuePair<string, Type>("Types", typeof(SO_Class_Type)),
          new KeyValuePair<string, Type>("Effects", typeof(SO_Item_Effect)),
          new KeyValuePair<string, Type>("Triggers", typeof(SO_Effect_Trigger)),
          new KeyValuePair<string, Type>("ItemSlots", typeof(SO_ItemSlot)),
          new KeyValuePair<string, Type>("ItemPools", null),
          new KeyValuePair<string, Type>("Stats", typeof(SO_Stat)),
           }, OnSubTabChanged, false, false, false, true);
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