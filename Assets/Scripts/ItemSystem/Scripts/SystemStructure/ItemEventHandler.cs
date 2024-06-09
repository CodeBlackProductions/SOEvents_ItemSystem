using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class ItemEventHandler : MonoBehaviour
{
    private static ItemEventHandler m_instance;

    public static ItemEventHandler Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<ItemEventHandler>();

                if (m_instance == null)
                {
                    GameObject singletonObject = new GameObject("ItemEventHandler");
                    m_instance = singletonObject.AddComponent<ItemEventHandler>();
                    DontDestroyOnLoad(singletonObject);
                }
            }

            return m_instance;
        }
    }

    public static event System.Action Initialized;

    private Dictionary<System.Type, ScriptableObject> events = new Dictionary<System.Type, ScriptableObject>();

    public void RegisterEvent<T>(T eventSO) where T : ScriptableObject
    {
        var type = eventSO.GetType();
        if (!events.ContainsKey(type))
        {
            events.Add(type, eventSO);
        }
    }

    public void RemoveEvent<T>(T eventSO) where T : ScriptableObject
    {
        var type = eventSO.GetType();
        if (events.ContainsKey(type))
        {
            events.Remove(type);
        }
    }

    public void InvokeEvent<T>(IItemUser _Source, IItemUser _Target) where T : SO_Effect_Trigger
    {
        T eventSO = GetEvent<T>();

        eventSO?.Invoke(_Source, _Target);
    }

    private T GetEvent<T>() where T : ScriptableObject
    {
        var type = typeof(T);
        if (events.ContainsKey(type))
        {
            return events[type] as T;
        }
        return null;
    }

    private void Awake()
    {
        Initialized?.Invoke();
    }
}