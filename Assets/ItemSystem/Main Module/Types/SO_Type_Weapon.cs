using ItemSystem.Editor;
using ItemSystem.MainModule;
using ItemSystem.SubModules;
using UnityEngine;

public enum EHands
{
    None, One, Two
}

public enum EType
{
    Melee, Ranged
}

public class Type_Weapon : SO_Class_Type
{
    [SerializeField] private GameObject m_Projectile = null;

    [ItemToolkitAccess] public GameObject Projectile { get => m_Projectile; set => m_Projectile = value; }
}