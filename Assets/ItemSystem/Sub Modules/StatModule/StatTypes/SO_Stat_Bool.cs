using ItemSystem.Editor;
using ItemSystem.SubModules;
using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[Serializable]
public class SO_Stat_Bool : SO_Stat_DynamicValue
{
    [SerializeField] private bool[] m_Value = new bool[] { true, false };

    [ItemToolkitAccess] public bool[] StatValue { get => m_Value; set => m_Value = value; }

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
        m_Value[_Index] = (bool)_Value;
    }
}