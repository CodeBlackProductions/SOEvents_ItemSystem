using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStat", menuName = "StatSystem/Stat/Stat_String")]
[Serializable]
public class SO_Stat_String : SO_Stat
{
    [SerializeField] private string m_Value = "NewStat";

    public override object GetValue()
    {
        return m_Value;
    }

    public override void SetValue(object value)
    {
        m_Value = value.ToString();
    }
}