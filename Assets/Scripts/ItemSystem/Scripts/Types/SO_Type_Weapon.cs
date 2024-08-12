using UnityEngine;

public enum EHands
{
    None, One, Two
}

public enum EType
{
    Melee, Ranged
}

[CreateAssetMenu(fileName = "New_Type", menuName = "ItemSystem/Class/Type/Weapon")]
public class Type_Weapon : SO_Class_Type
{
    [SerializeField] private EHands m_hands;
    [SerializeField] private EType m_Type;

    //Only show When Ranged
    [ConditionalHide(nameof(m_Type), 1)]
    [SerializeField] private float m_Range;

    [ConditionalHide(nameof(m_Type), 1)]
    [SerializeField] private float m_MinRange;

    [ConditionalHide(nameof(m_Type), 1)]
    [SerializeField] private GameObject m_Projectile;

    [SerializeField] private float m_Damage;
    [SerializeField] private float m_AtkCooldown;
}