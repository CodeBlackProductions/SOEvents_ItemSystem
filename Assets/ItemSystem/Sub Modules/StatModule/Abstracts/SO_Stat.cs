using ItemSystem.Editor;
using ItemSystem.MainModule;
using System;
using UnityEditor;
using UnityEngine;

namespace ItemSystem.SubModules
{
    /// <summary>
    /// Base class for user stats of different types.
    /// Inherit from this when creating a new base type of stat. (e.g. <see cref="SO_Stat_Float"/> , <see cref="SO_Stat_String"/>  etc.)
    /// </summary>
    [Serializable]
    public abstract class SO_Stat : ScriptableObject, IItemModule
    {
        //Name of the Stat Module
        [SerializeField] protected string m_StatName = "NewStat";

        //Name of the stat it should register to when utilized on a character. (E.g: different damage modules all addig onto "Damage")
        [SerializeField] protected string m_TargetUserStat = "NewStat";

        [SerializeField] protected GUID m_StatGUID;

        [ItemToolkitAccess] public string StatName { get => m_StatName; set => m_StatName = value; }
        [ItemToolkitAccess] public string TargetUserStat { get => m_TargetUserStat; set => m_TargetUserStat = value; }

        public string ModuleName { get => m_StatName; set => m_StatName = value; }
        public GUID ModuleGUID { get => m_StatGUID; set => m_StatGUID = value; }

        public abstract Type GetStatType();

        public abstract object GetStatValue();

        public abstract void SetStatValue(object value);
    }
}