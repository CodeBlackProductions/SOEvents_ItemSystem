using UnityEngine;

public enum EHands
{
    None, One, Two
}

public enum EType
{
    Melee, Ranged
}

[CreateAssetMenu(fileName = "New_Type", menuName = "ItemSystem/Item/Class/Type/Type_Weapon_OneHanded")]
public class Type_Weapon : SO_Class_Type
{
    [SerializeField] private EHands m_hands;
    [SerializeField] private EType m_Type;
    [SerializeField] private float m_Range;
    [SerializeField] private float m_MinRange;
    [SerializeField] private IProjectile m_Projectile;
    [SerializeField] private float m_Damage;
    [SerializeField] private float m_AtkCooldown;

    [ItemToolkitAccess] public EHands Hands { get => m_hands; set => m_hands = value; }
    [ItemToolkitAccess] public EType Type { get => m_Type; set => m_Type = value; }

    [ConditionalHide(nameof(Type), 1)]
    [ItemToolkitAccess] public float Range { get => m_Range; set => m_Range = value; }

    [ConditionalHide(nameof(Type), 1)]
    [ItemToolkitAccess] public float MinRange { get => m_MinRange; set => m_MinRange = value; }

    [ConditionalHide(nameof(Type), 1)]
    [ItemToolkitAccess] public IProjectile Projectile { get => m_Projectile; set => m_Projectile = value; }

    [ItemToolkitAccess] public float Damage { get => m_Damage; set => m_Damage = value; }
    [ItemToolkitAccess] public float AtkCooldown { get => m_AtkCooldown; set => m_AtkCooldown = value; }
}