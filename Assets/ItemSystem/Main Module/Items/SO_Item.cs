using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum ERarity
{
    Common, Uncommon, Rare, Legendary, Unique
}

/// <summary>
/// Core class of the entire system. Contains all necessary pieces to create a functioning item.
/// </summary>

public class SO_Item : ScriptableObject, IItemModule
{
    [SerializeField] private string m_ItemName = "NewItem";
    [SerializeField] private GUID m_ItemGUID;
    [SerializeField] private SO_Item_Class m_Class;
    [SerializeField] private ERarity m_Rarity;
    [SerializeField] private SO_Item_Effect[] m_Effects;

    private int m_TypeIndex;
    private string m_SlotType;
    private Dictionary<string, SO_Stat> m_Stats = new Dictionary<string, SO_Stat>();

    [ItemToolkitAccess] public SO_Item_Class Class { get => m_Class; set => m_Class = value; }
    [ItemToolkitAccess] public int TypeIndex { get => m_TypeIndex; set => m_TypeIndex = value; }
    [ItemToolkitAccess] public ERarity Rarity { get => m_Rarity; set => m_Rarity = value; }
    [ItemToolkitAccess] public SO_Item_Effect[] Effects { get => m_Effects; set => m_Effects = value; }
    [ItemToolkitAccess] public string ItemName { get => m_ItemName; set => m_ItemName = value; }
    [ItemToolkitAccess] public Dictionary<string, SO_Stat> Stats { get => m_Stats; set => m_Stats = value; }

    public string ModuleName { get => m_ItemName; set => m_ItemName = value; }
    public GUID ModuleGUID { get => m_ItemGUID; set => m_ItemGUID = value; }

    private void OnValidate()
    {
        if (m_Class != null)
        {
            m_SlotType = m_Class.ClassName;
        }
    }
}