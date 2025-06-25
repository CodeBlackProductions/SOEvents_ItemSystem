using ItemSystem.Editor;
using ItemSystem.SubModules;
using System;
using UnityEngine;

[Serializable]
public class SO_Stat_Bool : SO_Stat
{
    [SerializeField] private bool m_Value = false;

    [ItemToolkitAccess] public bool StatValue { get => m_Value; set => m_Value = value; }

    public override System.Type GetStatType()
    {
        return m_Value.GetType();
    }

    public override object GetStatValue()
    {
        return m_Value;
    }

    public override void SetStatValue(object value)
    {
        m_Value = (bool)value;
    }
}