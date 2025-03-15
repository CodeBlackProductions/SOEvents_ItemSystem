using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(fileName = "NewStat", menuName = "StatSystem/Stat/Stat_Float")]
[Serializable]
public class SO_Stat_Float : SO_Stat
{
    [SerializeField] private float m_Value = 0;

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
        if (float.TryParse(value.ToString(), out float result))
        {
            m_Value = result;
        }
    }
}