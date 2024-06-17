using UnityEngine;

[CreateAssetMenu(fileName = "New_Effect", menuName = "ItemSystem/Effect/BasicEffect")]
public class test : SO_Item_Effect
{
    protected override void ItemEffect(IItemUser _Source, IItemUser _Target)
    {
        Debug.Log("This is just a " + EffectName + " Source: " + _Source + " Target: " + _Target);
    }
}