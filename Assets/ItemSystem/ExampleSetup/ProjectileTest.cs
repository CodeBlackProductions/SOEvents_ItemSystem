using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTest : MonoBehaviour, IProjectile
{
    public string Name { get => gameObject.name;}
    public GameObject Prefab { get => gameObject;}

}
