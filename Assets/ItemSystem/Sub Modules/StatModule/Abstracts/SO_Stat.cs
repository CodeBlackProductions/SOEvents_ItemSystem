using ItemSystem.Editor;
using ItemSystem.MainModule;
using System;
using UnityEditor;
using UnityEngine;

namespace ItemSystem.SubModules
{
    /// <summary>
    /// Base class for user stats of different types.
    /// Do NOT use this class directly, instead use <see cref="SO_Stat_DynamicValue"/> or <see cref="SO_Stat_StaticValue"/>.
    /// </summary>
    [Serializable]
    public abstract class SO_Stat : ScriptableObject, IItemModule, IItemModuleBase
    {
        [SerializeField] protected string m_StatName = "NewStat";

        [Tooltip("Name of the stat it should register to when utilized on a character. (E.g: different damage modules all addig onto \"Damage\")")]
        [SerializeField] protected string m_TargetUserStat = "NewStat";

        [SerializeField] private SO_ToolTip[] m_ToolTips;

        [SerializeField] private SO_Tag[] m_Tags;

        [SerializeField] protected GUID m_StatGUID;

        [ItemToolkitAccess] public string StatName { get => m_StatName; set => m_StatName = value; }
        [ItemToolkitAccess] public string TargetUserStat { get => m_TargetUserStat; set => m_TargetUserStat = value; }
        [ItemToolkitAccess] public SO_ToolTip[] ToolTips { get => m_ToolTips; set => m_ToolTips = value; }
        [ItemToolkitAccess] public SO_Tag[] Tags { get => m_Tags; set => m_Tags = value; }

        public string ModuleName { get => m_StatName; set => m_StatName = value; }
        public GUID ModuleGUID { get => m_StatGUID; set => m_StatGUID = value; }
    }
}