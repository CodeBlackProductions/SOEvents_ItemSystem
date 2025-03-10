using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "New_Effect", menuName = "ItemSystem/Item/Effect/Effect_Basic")]
public class SO_Effect_Basic : SO_Item_Effect
{
    protected override void ItemEffect(IItemUser _Source, IItemUser _Target)
    {
        Debug.Log("This is just a " + EffectName + " Source: " + _Source + " Target: " + _Target);
    }
}