using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// Base class for user stats of different types.
/// Inherit from this when creating a new base type of stat. (e.g. <see cref="SO_Stat_Float"/> , <see cref="SO_Stat_String"/>  etc.)
/// </summary>
[Serializable]
public abstract class SO_Stat : ScriptableObject
{
    [SerializeField] protected string m_StatName = "NewStat";

    public abstract string GetStatName();

    public abstract Type GetStatType();

    public abstract object GetStatValue();

    public abstract void SetStatValue(object value);
}