using UnityEngine;
using ItemSystem.SubModules;
using System;

public class ProjectileTest : MonoBehaviour, IProjectile
{
    public string ProjectileName { get => gameObject.name; }
    public GameObject ProjectilePrefab { get => gameObject; }
}