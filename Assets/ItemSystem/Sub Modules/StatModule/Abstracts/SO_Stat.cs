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
        [SerializeField] protected string m_StatName = "NewStat";
        [SerializeField] protected string m_ModuleName = "NewStatModule";
        [SerializeField] protected GUID m_StatGUID;

        [ItemToolkitAccess] public string StatName { get => m_StatName; set => m_StatName = value; }
        [ItemToolkitAccess] public string ModuleName { get => m_ModuleName; set => m_ModuleName = value; }

        public GUID ModuleGUID { get => m_StatGUID; set => m_StatGUID = value; }

        public abstract Type GetStatType();

        public abstract object GetStatValue();

        public abstract void SetStatValue(object value);
    }
}