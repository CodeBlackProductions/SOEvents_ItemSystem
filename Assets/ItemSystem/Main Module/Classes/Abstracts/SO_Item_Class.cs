using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base item class, subcategory of <see cref="SO_Item"/> (e.g. Sword / weapon).
/// Inherit from this when creating new item classes.
/// </summary>
public abstract class SO_Item_Class : ScriptableObject
{
    [SerializeField] private string m_ClassName = "NewClass";
    [SerializeField] private SO_Class_Type[] m_Types;
    [SerializeField] private List<SO_Stat> m_ClassStats;

    private Dictionary<string, SO_Stat> m_Stats = new Dictionary<string, SO_Stat>();

    [ItemToolkitAccess] public string ClassName { get => m_ClassName; set => m_ClassName = value; }
    [ItemToolkitAccess] public SO_Class_Type[] Types { get => m_Types; set => m_Types = value; }
    [ItemToolkitAccess] public Dictionary<string, SO_Stat> Stats { get => m_Stats; set => m_Stats = value; }

    private void OnValidate()
    {
      
        if (m_ClassStats != null && m_ClassStats.Count > 0)
        {
            m_Stats.Clear();
            foreach (SO_Stat stat in m_ClassStats)
            {
                if (stat != null && !m_Stats.ContainsKey(stat.GetStatName()))
                {
                    m_Stats.Add(stat.GetStatName(), stat);
                }
            }
        }
    }
}