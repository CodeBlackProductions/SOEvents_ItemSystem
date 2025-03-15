using UnityEngine;

/// <summary>
/// Dummy class that runs all coroutines of the item system. Does nothing on its own.
/// </summary>
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
                    GameObject singletonObject = new GameObject("ItemCoroutineHandler");
                    m_instance = singletonObject.AddComponent<ItemCoroutineHandler>();

                }
            }

            return m_instance;
        }
    }
}