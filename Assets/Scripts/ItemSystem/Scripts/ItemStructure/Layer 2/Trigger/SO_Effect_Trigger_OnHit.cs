using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New_OnHit", menuName = "ItemSystem/Effect/Trigger/OnHit")]
public class SO_Effect_Trigger_OnHit : SO_Effect_Trigger
{
    protected override bool CheckCondition(IItemUser _Source, IItemUser _Target)
    {
        return true;
    }
}