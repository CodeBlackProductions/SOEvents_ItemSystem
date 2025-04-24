using ItemSystem.Editor;
using UnityEngine;

namespace ItemSystem.SubModules
{
    public interface IProjectile
    {
        [ItemToolkitAccess] public string ProjectileName { get; }
        [ItemToolkitAccess] public GameObject ProjectilePrefab { get; }
    }
}