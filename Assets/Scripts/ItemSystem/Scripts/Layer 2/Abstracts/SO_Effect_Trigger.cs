using System.Collections.Generic;
using UnityEngine;

public abstract class SO_Effect_Trigger : ScriptableObject
{
    [SerializeField] private string m_TriggerName = "NewTrigger";

    protected List<SO_Item_Effect> m_Listener = new List<SO_Item_Effect>();

    public string TriggerName { get => m_TriggerName; set => m_TriggerName = value; }

    public void OnEnable()
    {
        ItemEventHandler.Initialized += Initialize;
    }

    private void OnDisable()
    {
        ItemEventHandler.Instance?.RemoveEvent(this);
    }

    private void Initialize()
    {
        ItemEventHandler.Instance?.RegisterEvent(this);
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
            if (CheckCondition(_Source, _Target))
            {
                for (int i = m_Listener.Count - 1; i >= 0; i--)
                {
                    if (!CheckEffectRegistry(_Source, m_Listener[i]))
                    {
                        continue;
                    }

                    m_Listener[i]?.OnInvoke(_Source, _Target);
                }
            }
        }
    }

    protected bool CheckEffectRegistry(IItemUser _Source, SO_Item_Effect _Listener)
    {
        if (!_Source.EffectRegistry.ContainsKey(this)) 
        { 
            return false;
        }
        return _Source.EffectRegistry[this].Contains(_Listener);
    }

    protected abstract bool CheckCondition(IItemUser _Source, IItemUser _Target);
}