using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "New_ApplyStackEffect", menuName = "ItemSystem/Item/Effect/Effect_ApplyStack")]
public class SO_Effect_ApplyStacks : SO_Item_Effect
{
    [SerializeField] private So_Item_StackEffect m_StackEffect;

    protected override void ItemEffect(IItemUser _Source, IItemUser _Target)
    {
        Debug.Log(EffectName + " just applied stacks!");
        m_StackEffect.Trigger.Invoke(_Source, _Target);
    }
}