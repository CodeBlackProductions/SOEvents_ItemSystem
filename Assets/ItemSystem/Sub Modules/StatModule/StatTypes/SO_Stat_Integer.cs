using ItemSystem.Editor;
using ItemSystem.SubModules;
using System;
using UnityEngine;

[Serializable]
public class SO_Stat_Integer : SO_Stat
{
    [SerializeField] private int[] m_Value = new int[] { 0 };

    [ItemToolkitAccess] public int[] StatValue { get => m_Value; set => m_Value = value; }

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
        if (int.TryParse(_Value.ToString(), out int result))
        {
            m_Value[_Index] = result;
        }
    }
}