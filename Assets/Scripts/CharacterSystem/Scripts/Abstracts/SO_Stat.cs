using System;
using UnityEngine;

/// <summary>
/// Base class for user stats of different types.
/// Inherit from this when creating a new base type of stat. (e.g. <see cref="SO_Stat_Float"/> , <see cref="SO_Stat_String"/>  etc.)
/// </summary>
[Serializable]
public abstract class SO_Stat : ScriptableObject
{
    [SerializeField] private string m_StatName = "NewStat";

    public abstract object GetValue();

    public abstract void SetValue(object value);
}