using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This trigger adds and manages stacks for <see cref="So_Item_StackEffect"/>.
/// Call from another effect (e.g. On Hit Poison).
/// </summary>

public class SO_Effect_Trigger_Stack : SO_Effect_Trigger
{
    private Dictionary<IItemUser, Dictionary<So_Item_StackEffect, StackEffectData>> activeEffects = new Dictionary<IItemUser, Dictionary<So_Item_StackEffect, StackEffectData>>();

    /// <summary>
    /// Add/ Remove Stacks for <see cref="So_Item_StackEffect"/>.
    /// </summary>
    /// <param name="_Source"><see cref="IItemUser"/> that called the trigger</param>
    /// <param name="_Target"><see cref="IItemUser"/> that should get targeted by effect</param>
    /// <returns>Always returns false, since we dont want to call the effects multiple times.</returns>
    protected override bool CustomFunctionality(IItemUser _Source, IItemUser _Target)
    {
        foreach (var effect in m_Listener)
        {
            So_Item_StackEffect stackEffect = effect as So_Item_StackEffect;

            if (stackEffect == null)
            {
                continue;
            }

            if (Random.Range(0.0f, 1.0f) > stackEffect.ChanceToAddStacks)
            {
                continue;
            }

            if (!activeEffects.ContainsKey(_Target))
            {
                activeEffects[_Target] = new Dictionary<So_Item_StackEffect, StackEffectData>();
            }

            if (activeEffects[_Target].ContainsKey(stackEffect))
            {
                var stackData = activeEffects[_Target][stackEffect];
                stackData.StackCount += stackEffect.StacksAddedPerHit;
                if (stackEffect.MaxStacks != 0)
                {
                    if (stackData.StackCount > stackEffect.MaxStacks)
                    {
                        stackData.StackCount = stackEffect.MaxStacks;
                    }
                }
            }
            else
            {
                StackEffectData newEffect = new StackEffectData
                {
                    StackCount = stackEffect.StacksAddedPerHit,
                    EffectCoroutine = ItemCoroutineHandler.Instance.StartCoroutine(IntervalRoutine(_Source, _Target, stackEffect.Interval, stackEffect))
                };
                activeEffects[_Target][stackEffect] = newEffect;
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
    private IEnumerator IntervalRoutine(IItemUser _Source, IItemUser _Target, float _Interval, So_Item_StackEffect effect)
    {
        while (true)
        {
            yield return new WaitForSeconds(_Interval);
            InvokeInterval(_Source, _Target, effect);

            if (activeEffects[_Target].ContainsKey(effect))
            {
                var stackData = activeEffects[_Target][effect];
                stackData.StackCount -= effect.StacksConsumedPerInterval;
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
    private void InvokeInterval(IItemUser _Source, IItemUser _Target, So_Item_StackEffect effect)
    {
        if (effect == null)
        {
            throw new System.Exception("Effect not initialized in " + this.name);
        }

        if (CheckStackRegistry(_Source, _Target, effect))
        {
            effect?.OnStackInvoke(_Source, _Target, activeEffects[_Target][effect].StackCount);
        }
    }

    private bool CheckStackRegistry(IItemUser _Source, IItemUser _Target, So_Item_StackEffect effect)
    {
        return (activeEffects.ContainsKey(_Target) && activeEffects[_Target].ContainsKey(effect));
    }

    private class StackEffectData
    {
        public int StackCount;
        public Coroutine EffectCoroutine;
    }
}