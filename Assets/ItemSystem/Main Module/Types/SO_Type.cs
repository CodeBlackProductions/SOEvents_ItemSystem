using ItemSystem.Editor;
using ItemSystem.MainModule;
using UnityEngine;

public class Type : SO_Class_Type
{
    [SerializeField] private GameObject m_Projectile = null;

    [ItemToolkitAccess] public GameObject Projectile { get => m_Projectile; set => m_Projectile = value; }
}