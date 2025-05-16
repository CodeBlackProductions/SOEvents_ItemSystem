using UnityEngine;
using ItemSystem.Editor;
using ItemSystem.MainModule;

namespace ItemSystem
{
    public class SO_Effect_ApplyStacks : SO_Item_Effect
    {
        [SerializeField] private So_Item_StackEffect m_StackEffect;

        [ItemToolkitAccess] public So_Item_StackEffect StackEffect { get => m_StackEffect; set => m_StackEffect = value; }

        protected override void ItemEffect(IItemUser _Source, IItemUser _Target)
        {
            Debug.Log(EffectName + " just applied stacks!");
            (m_StackEffect.Trigger as SO_Effect_Trigger_Stack).Invoke(_Source, _Target, this);
        }
    }
}