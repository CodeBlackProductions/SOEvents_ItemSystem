using UnityEngine;
using ItemSystem.SubModules;

public class ProjectileTest : MonoBehaviour, IProjectile
{
    public string Name { get => gameObject.name; }
    public GameObject Prefab { get => gameObject; }
}