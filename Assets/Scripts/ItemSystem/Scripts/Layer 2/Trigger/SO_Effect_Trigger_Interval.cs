using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New_Interval", menuName = "ItemSystem/Effect/Trigger/Interval")]
public class SO_Effect_Trigger_Interval : SO_Effect_Trigger
{
    [SerializeField] private float m_Interval = 1.0f;

    private Dictionary<IItemUser, Coroutine> activeCoroutines = new Dictionary<IItemUser, Coroutine>();

    protected override bool CheckCondition(IItemUser _Source, IItemUser _Target)
    {
        if (activeCoroutines.ContainsKey(_Source))
        {
            ItemCoroutineHandler.Instance.StopCoroutine(activeCoroutines[_Source]);
            activeCoroutines.Remove(_Source);
            return false;
        }
        else
        {
            Coroutine newCoroutine = ItemCoroutineHandler.Instance.StartCoroutine(IntervalRoutine(_Source, _Target, m_Interval));
            activeCoroutines.Add(_Source, newCoroutine);
            return true;
        }
    }

    private IEnumerator IntervalRoutine(IItemUser _Source, IItemUser _Target, float _Interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(_Interval);
            InvokeInterval(_Source, _Target);
        }
    }

    private void InvokeInterval(IItemUser _Source, IItemUser _Target)
    {
        if (m_Listener == null)
        {
            throw new System.Exception("Listeners not initialized in " + this.name);
        }

        if (m_Listener.Count > 0)
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