using ItemSystem.Editor;
using ItemSystem.MainModule;
using UnityEditor;
using UnityEngine;

namespace ItemSystem.SubModules
{
    public class SO_ToolTip : ScriptableObject, IItemModule, IItemModuleBase
    {
        [SerializeField] private string m_ToolTipName = "New ToolTip";
        [SerializeField] private string m_ToolTipID = "New ID";
        [SerializeField] private string m_HyperlinkText = "New DisplayText";
        [SerializeField] private string m_ToolTipText = "New Text";
        [SerializeField] private SO_Tag[] m_Tags;
        [SerializeField] protected GUID m_TagGUID;

        [ItemToolkitAccess] public string ModuleName { get => m_ToolTipName; set => m_ToolTipName = value; }
        [ItemToolkitAccess] public string ToolTipID { get => m_ToolTipID; set => m_ToolTipID = value; }
        [ItemToolkitAccess] public string HyperlinkText { get => m_HyperlinkText; set => m_HyperlinkText = value; }
        [ItemToolkitAccess] public string ToolTipText { get => m_ToolTipText; set => m_ToolTipText = value; }
        [ItemToolkitAccess] public SO_Tag[] Tags { get => m_Tags; set => m_Tags = value; }

        public GUID ModuleGUID { get => m_TagGUID; set => m_TagGUID = value; }
    }
}