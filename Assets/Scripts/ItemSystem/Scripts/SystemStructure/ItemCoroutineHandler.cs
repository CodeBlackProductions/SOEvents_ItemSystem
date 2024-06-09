using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCoroutineHandler : MonoBehaviour
{
    private static ItemCoroutineHandler m_instance;
    public static ItemCoroutineHandler Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<ItemCoroutineHandler>();

                if (m_instance == null)
                {
                    GameObject singletonObject = new GameObject("ItemEventHandler");
                    m_instance = singletonObject.AddComponent<ItemCoroutineHandler>();
                    DontDestroyOnLoad(singletonObject);
                }
            }

            return m_instance;
        }
    }
}