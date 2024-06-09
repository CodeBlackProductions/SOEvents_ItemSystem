using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class SO_Stat : ScriptableObject
{
    [SerializeField] private string m_StatName = "NewStat";

    public abstract object GetValue();
    public abstract void SetValue(object value);
}
