using System.Collections.Generic;
using UnityEngine;

public class ItemEventHandler : MonoBehaviour
{
    private static ItemEventHandler _instance;

    public static ItemEventHandler Instance { get => _instance; }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private Dictionary<System.Type, ScriptableObject> events = new Dictionary<System.Type, ScriptableObject>();

    public void RegisterEvent<T>(T eventSO) where T : ScriptableObject
    {
        var type = typeof(T);
        if (!events.ContainsKey(type))
        {
            events[type] = eventSO;
        }
    }

    public void RemoveEvent<T>(T eventSO) where T : ScriptableObject
    {
        var type = typeof(T);
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
}