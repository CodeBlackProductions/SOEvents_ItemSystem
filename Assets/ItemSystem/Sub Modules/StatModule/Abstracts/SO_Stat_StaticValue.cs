using ItemSystem.Editor;
using ItemSystem.MainModule;
using System;
using UnityEditor;
using UnityEngine;

namespace ItemSystem.SubModules
{
    /// <summary>
    /// Base class for user stats of different types.
    /// This class is used to define a singular static value rather than multiple possible Values.
    /// Inherit from this when creating a new base type of stat. (e.g. <see cref="SO_Stat_Float"/> , <see cref="SO_Stat_String"/>  etc.)
    /// </summary>
    [Serializable]
    public abstract class SO_Stat_StaticValue : SO_Stat_Base, IItemModule, IItemModuleBase
    {
        public abstract System.Type GetStatType();

        public abstract object GetStatValue();

        public abstract void SetStatValue(object _Value);
    }
}