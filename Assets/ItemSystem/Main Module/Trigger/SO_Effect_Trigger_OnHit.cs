using ItemSystem.MainModule;
using ItemSystem.SubModules;
using UnityEngine;

public class SO_Effect_Trigger_OnHit : SO_Effect_Trigger
{
    // Override this to do custom checks before calling custom functionality and/or listeners.
    //always returns true if not customized
    protected override bool CheckCondition(IItemUser _Source, IItemUser _Target)
    {
        Runtime_Stat health = _Source.UserStats[EUserStats.Health.ToString()];
        Runtime_Stat damage = _Target.UserStats[EUserStats.Damage.ToString()];

        if ((float)health.Value <= (float)damage.Value)
        {
            Debug.LogWarning("Omae wa mou shindeiru");
            return false;
        }
        return true;
    }

 
    // Override this to add custom functionality to your trigger.
    //Return true if you want to call registered listeners. Return false if you do so manually in here. Or do both if you're a madman.
    protected override bool CustomFunctionality(IItemUser _Source, IItemUser _Target)
    {
        return true;
    }
}