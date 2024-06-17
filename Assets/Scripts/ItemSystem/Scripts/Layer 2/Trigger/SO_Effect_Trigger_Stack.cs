using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This trigger adds and manages stacks for stackable <see cref="SO_Item_Effect"/>.
/// Call from another effect (e.g. On Hit Poison).
/// </summary>
[CreateAssetMenu(fileName = "New_StackTrigger", menuName = "ItemSystem/Effect/Trigger/Stack")]
public class SO_Effect_Trigger_Stack : SO_Effect_Trigger
{
    [SerializeField] private float m_Interval = 1.0f;
    [SerializeField] [Range(0.0f,1.0f)] private float m_ChanceToAddStacks = 1.0f;
    [SerializeField] private int m_StacksAddedPerHit = 1;
    [SerializeField] private int m_MaxStacks = 0;
    [SerializeField] private int m_StacksConsumedPerInterval = 1;

    private Dictionary<IItemUser, Dictionary<SO_Item_Effect, StackEffectData>> activeEffects = new Dictionary<IItemUser, Dictionary<SO_Item_Effect, StackEffectData>>();

    /// <summary>
    /// Add/ Remove Stacks for stackable <see cref="SO_Item_Effect"/>.
    /// </summary>
    /// <param name="_Source"><see cref="IItemUser"/> that called the trigger</param>
    /// <param name="_Target"><see cref="IItemUser"/> that should get targeted by effect</param>
    /// <returns>Always returns false, since we dont want to call the effects multiple times.</returns>
    protected override bool CustomFunctionality(IItemUser _Source, IItemUser _Target)
    {
        foreach (var effect in m_Listener)
        {
            if (!activeEffects.ContainsKey(_Target))
            {
                activeEffects[_Target] = new Dictionary<SO_Item_Effect, StackEffectData>();
            }

            if (activeEffects[_Target].ContainsKey(effect))
            {
                var stackData = activeEffects[_Target][effect];
                stackData.StackCount += m_StacksAddedPerHit;
                if (m_MaxStacks != 0)
                {
                    if (stackData.StackCount > m_MaxStacks)
                    {
                        stackData.StackCount = m_MaxStacks;
                    }
                }
               
            }
            else
            {
                StackEffectData newEffect = new StackEffectData
                {
                    StackCount = m_StacksAddedPerHit,
                    EffectCoroutine = ItemCoroutineHandler.Instance.StartCoroutine(IntervalRoutine(_Source, _Target, m_Interval, effect))
                };
                activeEffects[_Target][effect] = newEffect;
            }
        }
        return false;
    }
    /// <summary>
    /// Coroutine that invokes the corresponding effects based on stack amount.
    /// </summary>
    /// <param name="_Source"><see cref="IItemUser"/> that called the trigger</param>
    /// <param name="_Target"><see cref="IItemUser"/> that should get targeted by effect</param>
    /// <param name="_Interval">time between invokes</param>
    /// <returns></returns>
    private IEnumerator IntervalRoutine(IItemUser _Source, IItemUser _Target, float _Interval, SO_Item_Effect effect)
    {
        while (true)
        {
            yield return new WaitForSeconds(_Interval);
            InvokeInterval(_Source, _Target, effect);

            if (activeEffects[_Target].ContainsKey(effect))
            {
                var stackData = activeEffects[_Target][effect];
                stackData.StackCount -= m_StacksConsumedPerInterval;
                if (stackData.StackCount <= 0)
                {
                    ItemCoroutineHandler.Instance.StopCoroutine(stackData.EffectCoroutine);
                    activeEffects[_Target].Remove(effect);
                    if (activeEffects[_Target].Count == 0)
                    {
                        activeEffects.Remove(_Target);
                    }
                    break;
                }
            }
        }
    }
    /// <summary>
    /// Invokes the corresponding effects. Gets called by the coroutines.
    /// </summary>
    /// <param name="_Source"><see cref="IItemUser"/> that called the trigger</param>
    /// <param name="_Target"><see cref="IItemUser"/> that should get targeted by effect</param>
    /// <exception cref="System.Exception"></exception>
    private void InvokeInterval(IItemUser _Source, IItemUser _Target, SO_Item_Effect effect)
    {
        if (effect == null)
        {
            throw new System.Exception("Effect not initialized in " + this.name);
        }

        if (CheckEffectRegistry(_Source, _Target, effect))
        {
            effect?.OnInvoke(_Source, _Target);
        }
    }

    private bool CheckEffectRegistry(IItemUser _Source, IItemUser _Target, SO_Item_Effect effect)
    {
        return _Source.EffectRegistry.ContainsKey(this) && _Source.EffectRegistry[this].Contains(effect);
    }

    private class StackEffectData
    {
        public int StackCount;
        public Coroutine EffectCoroutine;
    }
}