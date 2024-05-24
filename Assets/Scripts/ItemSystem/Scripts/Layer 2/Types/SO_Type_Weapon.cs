using UnityEngine;

public enum EHands
{
    None, One, Two
}

public enum EType
{
    Melee, Ranged
}

[CreateAssetMenu(fileName = "New_Type", menuName = "ItemSystem/Class/Type_Weapon")]
public class Type_Weapon : SO_Class_Type
{
    [SerializeField] private EHands m_hands;
    [SerializeField] private EType m_Type;

    //[SerializeField] private float m_Range;
    //[SerializeField] private float m_MinRange;
    //[SerializeField] private GameObject m_Projectile;
    //[SerializeField] private float m_Damage;
    //[SerializeField] private float m_AtkCooldown;

    //where to put those?
}