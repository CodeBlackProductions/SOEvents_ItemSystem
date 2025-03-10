using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public enum ETarget
{
    Self, Target, TargetsInRangeSelf, TargetsInRangeTarget
}

public enum EAllowedTargets
{
    All, AllyOnly, EnemyOnly
}

/// <summary>
/// Base item effect. Contains all necessary elements to add additional effects to items. (e.g. Poison or Stat boosts)
/// Inherit from this when creating new effects.
/// </summary>
public abstract class SO_Item_Effect : ScriptableObject
{
    [SerializeField] private string m_EffectName = "NewEffect";
    [SerializeField] private SO_Effect_Trigger m_Trigger;
    [SerializeField] private ETarget m_EffectTarget;

    [ConditionalHide(nameof(m_EffectTarget), 1, 2, 3)]
    [SerializeField] private EAllowedTargets m_AllowedEffectTargets;

    [ConditionalHide(nameof(m_EffectTarget), 2, 3)]
    [SerializeField] private float m_TargetRange = 0;

    public string EffectName { get => m_EffectName; }
    public ETarget EffectTarget { get => m_EffectTarget; }
    public EAllowedTargets AllowedEffectTargets { get => m_AllowedEffectTargets; }
    public float TargetRange { get => m_TargetRange; }
    public SO_Effect_Trigger Trigger { get => m_Trigger; }

    private void OnEnable()
    {
        m_Trigger?.RegisterEffect(this);
    }

    private void OnDisable()
    {
        m_Trigger?.RemoveEffect(this);
    }

    /// <summary>
    /// Calls the actual effect when the corresponding <see cref="SO_Effect_Trigger"/> got activated.
    /// </summary>
    /// <param name="_Source"><see cref="IItemUser"/> that called the effect</param>
    /// <param name="_Target"><see cref="IItemUser"/> that gets hit by the effect</param>
    public void OnInvoke(IItemUser _Source, IItemUser _Target)
    {
        List<IItemUser> targets = GetTargetsInRange(_Source.ImplementingUser.transform.position, m_TargetRange);

        switch (m_EffectTarget)
        {
            case ETarget.Self:
                ItemEffect(_Source, _Source);
                break;

            case ETarget.Target:
                ItemEffect(_Source, _Target);
                break;

            case ETarget.TargetsInRangeSelf:

                foreach (var target in targets)
                {
                    switch (AllowedEffectTargets)
                    {
                        case EAllowedTargets.All:
                            ItemEffect(_Source, target);
                            break;

                        case EAllowedTargets.AllyOnly:
                            if (_Source.Team == target.Team)
                            {
                                ItemEffect(_Source, target);
                            }
                            break;

                        case EAllowedTargets.EnemyOnly:
                            if (_Source.Team != target.Team)
                            {
                                ItemEffect(_Source, target);
                            }
                            break;

                        default:
                            ItemEffect(_Source, target);
                            break;
                    }
                }
                break;

            case ETarget.TargetsInRangeTarget:

                targets = GetTargetsInRange(_Target.ImplementingUser.transform.position, m_TargetRange);
                foreach (var target in targets)
                {
                    switch (AllowedEffectTargets)
                    {
                        case EAllowedTargets.All:
                            ItemEffect(_Source, target);
                            break;

                        case EAllowedTargets.AllyOnly:
                            if (_Source.Team == target.Team)
                            {
                                ItemEffect(_Source, target);
                            }
                            break;

                        case EAllowedTargets.EnemyOnly:
                            if (_Source.Team != target.Team)
                            {
                                ItemEffect(_Source, target);
                            }
                            break;

                        default:
                            ItemEffect(_Source, target);
                            break;
                    }
                }
                break;

            default:
                ItemEffect(_Source, _Source);
                break;
        }
    }

    /// <summary>
    /// Override this to add your custom item effect. You can access the characters stats through <see cref="IItemUser.UserStats"/> .
    /// </summary>
    /// <param name="_Source"><see cref="IItemUser"/> that called the effect</param>
    /// <param name="_Target"><see cref="IItemUser"/> that gets hit by the effect</param>
    protected abstract void ItemEffect(IItemUser _Source, IItemUser _Target);

    private List<IItemUser> GetTargetsInRange(Vector3 _StartPoint, float _Range)
    {
        Collider[] colliders = Physics.OverlapSphere(_StartPoint, _Range);
        List<IItemUser> targets = new List<IItemUser>();
        foreach (Collider collider in colliders)
        {
            IItemUser itemUser;
            if (collider.TryGetComponent<IItemUser>(out itemUser))
            {
                targets.Add(itemUser);
            }
        }
        return targets;
    }
}