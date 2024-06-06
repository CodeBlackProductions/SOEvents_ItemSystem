using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCoroutineHandler : MonoBehaviour
{
    private static ItemCoroutineHandler m_instance;
    public static ItemCoroutineHandler Instance { get => m_instance; }

    private void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}