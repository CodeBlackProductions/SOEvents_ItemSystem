using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New_Effect", menuName = "ItemSystem/Effect/StackEffect")]
public class TestStackEffect : So_Item_StackEffect
{
    protected override void ItemEffect(IItemUser _Source, IItemUser _Target)
    {
        return;
    }

    protected override void ItemStackEffect(IItemUser _Source, IItemUser _Target, int _StackAmount)
    {
        Debug.Log("This is just a " + EffectName + " Source: " + _Source + " Target: " + _Target + " Stacks: " + _StackAmount);
    }
}
