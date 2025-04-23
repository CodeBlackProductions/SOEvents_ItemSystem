using ItemSystem.Editor;
using ItemSystem.SubModules;
using System;
using UnityEngine;

[Serializable]
public class SO_Stat_Float : SO_Stat
{
    [SerializeField] private float m_Value = 0;

    [ItemToolkitAccess] public float StatValue { get => m_Value; set => m_Value = value; }

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
        if (float.TryParse(value.ToString(), out float result))
        {
            m_Value = result;
        }
    }
}