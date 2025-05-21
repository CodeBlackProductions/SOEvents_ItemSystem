using ItemSystem.Editor;
using ItemSystem.MainModule;
using UnityEditor;
using UnityEngine;

namespace ItemSystem.SubModules
{
    public class SO_Tag : ScriptableObject, IItemModule, IItemModuleBase
    {
        [SerializeField] private string m_TagName = "NewTag";

        [SerializeField] protected GUID m_TagGUID;

        [ItemToolkitAccess] public string ModuleName { get => m_TagName; set => m_TagName = value; }
        public GUID ModuleGUID { get => m_TagGUID; set => m_TagGUID = value; }
    }
}