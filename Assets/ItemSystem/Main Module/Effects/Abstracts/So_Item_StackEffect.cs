using ItemSystem.Editor;
using UnityEngine;

namespace ItemSystem.MainModule
{
    /// <summary>
    /// Base stackable item effect. Contains all necessary elements to add additional stackable effects to items. (e.g. Poison or bleed)
    /// Inherit from this when creating new stackable effects.
    /// </summary>
    public abstract class So_Item_StackEffect : SO_Item_Effect
    {
        [SerializeField] private float m_Interval = 1.0f;
        [SerializeField][Range(0.0f, 1.0f)] private float m_ChanceToAddStacks = 1.0f;
        [SerializeField] private int m_StacksAddedPerHit = 1;
        [SerializeField] private int m_MaxStacks = 0;
        [SerializeField] private int m_StacksConsumedPerInterval = 1;

        [ItemToolkitAccess] public float Interval { get => m_Interval; set => m_Interval = value; }
        [ItemToolkitAccess] public float ChanceToAddStacks { get => m_ChanceToAddStacks; set => m_ChanceToAddStacks = value; }
        [ItemToolkitAccess] public int StacksAddedPerHit { get => m_StacksAddedPerHit; set => m_StacksAddedPerHit = value; }
        [ItemToolkitAccess] public int MaxStacks { get => m_MaxStacks; set => m_MaxStacks = value; }
        [ItemToolkitAccess] public int StacksConsumedPerInterval { get => m_StacksConsumedPerInterval; set => m_StacksConsumedPerInterval = value; }

        public void OnStackInvoke(IItemUser _Source, IItemUser _Target, int _StackAmount)
        {
            switch (EffectTarget)
            {
                case ETarget.Self:
                    ItemStackEffect(_Source, _Source, _StackAmount);
                    break;

                case ETarget.Target:
                    ItemStackEffect(_Source, _Target, _StackAmount);
                    break;

                case ETarget.TargetsInRangeSelf:

                    Collider[] targetsSelf = Physics.OverlapSphere(_Source.ImplementingUser.transform.position, TargetRange);
                    foreach (Collider target in targetsSelf)
                    {
                        IItemUser itemUser;
                        if (target.TryGetComponent<IItemUser>(out itemUser))
                        {
                            ItemStackEffect(_Source, itemUser, _StackAmount);
                        }
                    }
                    break;

                case ETarget.TargetsInRangeTarget:

                    Collider[] targets = Physics.OverlapSphere(_Target.ImplementingUser.transform.position, TargetRange);
                    foreach (Collider target in targets)
                    {
                        IItemUser itemUser;
                        if (target.TryGetComponent<IItemUser>(out itemUser))
                        {
                            ItemStackEffect(_Source, itemUser, _StackAmount);
                        }
                    }
                    break;

                default:
                    ItemStackEffect(_Source, _Target, _StackAmount);
                    break;
            }
        }

        /// <summary>
        /// Override this to add your custom item stack effect. You can access the characters stats through <see cref="IItemUser.UserStats"/> .
        /// Leave the ItemEffect method empty / make it instantly return, if you dont want it to have a regular / non stackable effect as well.
        /// </summary>
        /// <param name="_Source"><see cref="IItemUser"/> that called the effect</param>
        /// <param name="_Target"><see cref="IItemUser"/> that gets hit by the effect</param>
        protected abstract void ItemStackEffect(IItemUser _Source, IItemUser _Target, int _StackAmount);
    }
}