using ItemSystem.Editor;
using ItemSystem.SubModules;
using System;
using UnityEngine;

[Serializable]
public class SO_Stat_Color : SO_Stat
{
    [SerializeField] private Color[] m_Value = new Color[] { Color.white };

    [ItemToolkitAccess] public Color[] StatValue { get => m_Value; set => m_Value = value; }

    public override int GetStatCount()
    {
        return m_Value.Length;
    }

    public override System.Type GetStatType()
    {
        return m_Value.GetType().GetElementType();
    }

    public override object GetStatValue(int _Index)
    {
        return m_Value[_Index];
    }

    public override void SetStatValue(object _Value, int _Index)
    {
        m_Value[_Index] = (Color)_Value;
    }
}