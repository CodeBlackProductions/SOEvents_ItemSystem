using ItemSystem.Editor;
using ItemSystem.SubModules;
using System;
using UnityEngine;

[Serializable]
public class SO_Stat_Vector2 : SO_Stat
{
    [SerializeField] private Vector2 m_Value = Vector2.zero;

    [ItemToolkitAccess] public Vector2 StatValue { get => m_Value; set => m_Value = value; }

    public override Type GetStatType()
    {
        return m_Value.GetType();
    }

    public override object GetStatValue()
    {
        return m_Value;
    }

    public override void SetStatValue(object value)
    {
        m_Value = (Vector2)value;
    }
}