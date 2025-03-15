using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStat", menuName = "StatSystem/Stat/Stat_Integer")]
[Serializable]
public class SO_Stat_Integer : SO_Stat
{
    [SerializeField] private int m_Value = 0;

    public override string GetStatName()
    {
        return m_StatName;
    }

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