using ItemSystem.Editor;
using ItemSystem.SubModules;
using System;
using UnityEngine;

[Serializable]
public class SO_Stat_Integer : SO_Stat
{
    [SerializeField] private int m_Value = 0;

    [ItemToolkitAccess] public int StatValue { get => m_Value; set => m_Value = value; }

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
        if (int.TryParse(value.ToString(), out int result))
        {
            m_Value = result;
        }
    }
}