using ItemSystem.Editor;
using ItemSystem.MainModule;
using System;
using UnityEditor;
using UnityEngine;

namespace ItemSystem.SubModules
{
    /// <summary>
    /// Base class for user stats of different types.
    /// This class is used to define a collection of possible values for a singular stat. (e.g. multiple possible damage types, resistances, etc.)
    /// Inherit from this when creating a new base type of stat. (e.g. <see cref="SO_Stat_Float"/> , <see cref="SO_Stat_String"/>  etc.)
    /// </summary>
    [Serializable]
    public abstract class SO_Stat_DynamicValue : SO_Stat, IItemModule, IItemModuleBase
    {
        public abstract System.Type GetStatType();

        public abstract object GetStatValue(int _Index);

        public abstract int GetStatCount();

        public abstract void SetStatValue(object _Value, int _Index);
    }
}