using UnityEngine;

public interface IProjectile
{
    [ItemToolkitAccess] public string Name { get; set; }
    [ItemToolkitAccess] public GameObject Prefab { get; set; }
}