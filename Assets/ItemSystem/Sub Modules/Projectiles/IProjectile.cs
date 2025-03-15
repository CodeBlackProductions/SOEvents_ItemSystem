using UnityEngine;

public interface IProjectile
{
    [ItemToolkitAccess] public string Name { get;}
    [ItemToolkitAccess] public GameObject Prefab { get;}
}