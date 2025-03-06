using System.Collections.Generic;
using UnityEngine;

public enum ERarity
{
    Common, Uncommon, Rare, Legendary, Unique
}

/// <summary>
/// Core class of the entire system. Contains all necessary pieces to create a functioning item.
/// </summary>
[CreateAssetMenu(fileName = "New_Item", menuName = "ItemSystem/Item/Item")]
public class SO_Item : ScriptableObject
{
    [SerializeField] private string m_ItemName = "NewItem";
    [SerializeField] private SO_Item_Class m_Class;
    [SerializeField] private ERarity m_Rarity;
    [SerializeField] private SO_Item_Effect[] m_Effects;
    [SerializeField] private List<SO_Stat> m_ItemStats;

    private int m_TypeIndex;
    private string m_SlotType;
    private Dictionary<string, SO_Stat> m_Stats = new Dictionary<string, SO_Stat>();

    public SO_Item_Class Class { get => m_Class; set => m_Class = value; }
    public int TypeIndex { get => m_TypeIndex; set => m_TypeIndex = value; }
    public ERarity Rarity { get => m_Rarity; set => m_Rarity = value; }
    public SO_Item_Effect[] Effects { get => m_Effects; set => m_Effects = value; }
    public string ItemName { get => m_ItemName; set => m_ItemName = value; }
    public Dictionary<string, SO_Stat> Stats { get => m_Stats; set => m_Stats = value; }

    private void OnValidate()
    {
        if (m_Class != null)
        {
            m_SlotType = m_Class.ClassName;
        }

        if (m_ItemStats != null && m_ItemStats.Count > 0)
        {
            m_Stats.Clear();
            foreach (SO_Stat stat in m_ItemStats)
            {
                if (stat != null && !m_Stats.ContainsKey(stat.GetName()))
                {
                    m_Stats.Add(stat.GetName(), stat);
                }
            }
        }
    }
}