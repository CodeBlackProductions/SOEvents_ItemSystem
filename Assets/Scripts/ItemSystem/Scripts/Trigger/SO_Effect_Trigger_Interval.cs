using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This trigger calls each registered <see cref="SO_Item_Effect"/> in fixed intervals.
/// Call at character initialization.
/// </summary>
[CreateAssetMenu(fileName = "New_Interval", menuName = "ItemSystem/Item/Effect/Trigger/Trigger_Interval")]
public class SO_Effect_Trigger_Interval : SO_Effect_Trigger
{
    [SerializeField] private float m_Interval = 1.0f;

    private Dictionary<IItemUser, Coroutine> activeCoroutines = new Dictionary<IItemUser, Coroutine>();

    /// <summary>
    /// Add/ Remove Coroutine for effects.
    /// </summary>
    /// <param name="_Source"><see cref="IItemUser"/> that called the trigger</param>
    /// <param name="_Target"><see cref="IItemUser"/> that should get targeted by effect</param>
    /// <returns>true if new coroutine is registered, false if coroutine got removed</returns>
    protected override bool CustomFunctionality(IItemUser _Source, IItemUser _Target)
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
            return false;
        }
    }
    /// <summary>
    /// Coroutine that invokes the corresponding effects.
    /// </summary>
    /// <param name="_Source"><see cref="IItemUser"/> that called the trigger</param>
    /// <param name="_Target"><see cref="IItemUser"/> that should get targeted by effect</param>
    /// <param name="_Interval">time between invokes</param>
    /// <returns></returns>
    private IEnumerator IntervalRoutine(IItemUser _Source, IItemUser _Target, float _Interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(_Interval);
            InvokeInterval(_Source, _Target);
        }
    }
    /// <summary>
    /// Invokes the corresponding effects. Gets called by the coroutines.
    /// </summary>
    /// <param name="_Source"><see cref="IItemUser"/> that called the trigger</param>
    /// <param name="_Target"><see cref="IItemUser"/> that should get targeted by effect</param>
    /// <exception cref="System.Exception"></exception>
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