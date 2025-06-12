using ItemSystem.Editor;
using ItemSystem.MainModule;
using UnityEditor;
using UnityEngine;

namespace ItemSystem.SubModules
{
    public class SO_ToolTip : ScriptableObject, IItemModule, IItemModuleBase
    {
        [SerializeField] private string m_ToolTipName = "NewToolTip";
        [SerializeField] private string m_ToolTipID = "NewID";
        [SerializeField] private string m_ToolTipText = "NewText";

        [SerializeField] protected GUID m_TagGUID;

        [ItemToolkitAccess] public string ModuleName { get => m_ToolTipName; set => m_ToolTipName = value; }
        [ItemToolkitAccess] public string ToolTipID { get => m_ToolTipID; set => m_ToolTipID = value; }
        [ItemToolkitAccess] public string ToolTipText { get => m_ToolTipText; set => m_ToolTipText = value; }

        public GUID ModuleGUID { get => m_TagGUID; set => m_TagGUID = value; }
    }
}