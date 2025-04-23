using ItemSystem.Editor;
using UnityEngine;

namespace ItemSystem.SubModules
{
    public interface IProjectile
    {
        [ItemToolkitAccess] public string Name { get; }
        [ItemToolkitAccess] public GameObject Prefab { get; }
    }
}