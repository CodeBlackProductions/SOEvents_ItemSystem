using UnityEngine;
using ItemSystem.MainModule;

public class SO_Effect_Basic : SO_Item_Effect
{
    protected override void ItemEffect(IItemUser _Source, IItemUser _Target)
    {
        Debug.Log("This is just a " + EffectName + " Source: " + _Source + " Target: " + _Target);
    }
}