using ItemSystem.Editor;
using ItemSystem.SubModules;
using System;
using UnityEngine;

[Serializable]
public class SO_Stat_String : SO_Stat
{
    [SerializeField] private string m_Value = "NewStat";

    [ItemToolkitAccess] public string StatValue { get => m_Value; set => m_Value = value; }

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
        m_Value = value.ToString();
    }
}