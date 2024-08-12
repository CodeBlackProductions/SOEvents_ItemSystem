using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStat", menuName = "StatSystem/Stat/Stat_Integer")]
[Serializable]
public class SO_Stat_Integer : SO_Stat
{
    [SerializeField] private int m_Value = 0;

    public override object GetValue()
    {
        return m_Value;
    }

    public override void SetValue(object value)
    {
        if (int.TryParse(value.ToString(), out int result))
        {
            m_Value = result;
        }
    }
}