using UnityEngine;

public enum ERarity
{
    Common, Uncommon, Rare, Legendary, Unique
}

[CreateAssetMenu(fileName = "New_Item", menuName = "ItemSystem/Item")]
public class SO_Item : ScriptableObject
{
    [SerializeField] private SO_Item_Class m_Class;
    [SerializeField] private ERarity m_Rarity;
    [SerializeField] private SO_Item_Effect[] m_Effects;
    [SerializeField] private int m_GoldValue;

    private string m_SlotType;

    public SO_Item_Class Class { get => m_Class; set => m_Class = value; }
    public ERarity Rarity { get => m_Rarity; set => m_Rarity = value; }
    public SO_Item_Effect[] Effects { get => m_Effects; set => m_Effects = value; }
    public int GoldValue { get => m_GoldValue; set => m_GoldValue = value; }

    private void OnValidate()
    {
        if (m_Class != null)
        {
            m_SlotType = m_Class.ClassName;
        }
    }
}