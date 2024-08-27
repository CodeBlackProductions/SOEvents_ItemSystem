using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUserSpawner : MonoBehaviour
{
    [SerializeField] int m_NumberToSpawn = 1;
    [SerializeField] GameObject m_Prefab;

    private void Start()
    {
        for (int i = 0; i < m_NumberToSpawn; i++) 
        {
            Instantiate<GameObject>(m_Prefab);
        }
    }
}
