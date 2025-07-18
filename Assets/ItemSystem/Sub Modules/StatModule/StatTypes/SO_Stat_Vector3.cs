using ItemSystem.Editor;
using ItemSystem.SubModules;
using System;
using UnityEngine;

[Serializable]
public class SO_Stat_Vector3 : SO_Stat_DynamicValue
{
    [SerializeField] private Vector3[] m_Value = new Vector3[] { Vector3.zero };

    [ItemToolkitAccess] public Vector3[] StatValue { get => m_Value; set => m_Value = value; }

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
        m_Value[_Index] = (Vector3)_Value;
    }
}