using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New_Trigger", menuName = "ItemSystem/Effect/Trigger/Trigger")]
public class SO_Effect_Trigger : ScriptableObject
{
    [SerializeField] private string m_TriggerName = "NewTrigger";

    private List<SO_Item_Effect> m_Listener = new List<SO_Item_Effect>();

    public string TriggerName { get => m_TriggerName; set => m_TriggerName = value; }

    private void OnEnable()
    {
        ItemEventHandler.Instance?.RegisterEvent(this);
    }

    private void OnDisable()
    {
        ItemEventHandler.Instance?.RemoveEvent(this);
    }

    public void RegisterEffect(SO_Item_Effect _Effect)
    {
        m_Listener.Add(_Effect);
    }

    public void RemoveEffect(SO_Item_Effect _Effect)
    {
        m_Listener.Remove(_Effect);
    }

    public void Invoke(IItemUser _Source, IItemUser _Target)
    {
        if (m_Listener == null)
        {
            throw new System.Exception("Listeners not initialized in " + this.name);
        }

        if (m_Listener.Count > 0)
        {
            for (int i = m_Listener.Count - 1; i >= 0; i--)
            {
                m_Listener[i]?.OnInvoke(_Source, _Target);
            }
        }
    }
}