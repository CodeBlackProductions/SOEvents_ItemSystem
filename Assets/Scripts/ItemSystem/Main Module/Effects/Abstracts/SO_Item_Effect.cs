using UnityEngine;

public enum ETarget
{
    Self, Target, TargetsInRangeSelf, TargetsInRangeTarget
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

    [ConditionalHide(nameof(m_EffectTarget), 2, 3)]
    [SerializeField] private float m_TargetRange = 0;

    public string EffectName { get => m_EffectName; }
    public ETarget EffectTarget { get => m_EffectTarget; }
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
        switch (m_EffectTarget)
        {
            case ETarget.Self:
                ItemEffect(_Source, _Source);
                break;

            case ETarget.Target:
                ItemEffect(_Source, _Target);
                break;

            case ETarget.TargetsInRangeSelf:

                Collider[] targetsSelf = Physics.OverlapSphere(_Source.m_ImplementingUser.transform.position, m_TargetRange);
                foreach (Collider target in targetsSelf)
                {
                    IItemUser itemUser;
                    if (target.TryGetComponent<IItemUser>(out itemUser))
                    {
                        ItemEffect(_Source, itemUser);
                    }
                }
                break;

            case ETarget.TargetsInRangeTarget:

                Collider[] targets = Physics.OverlapSphere(_Target.m_ImplementingUser.transform.position, m_TargetRange);
                foreach (Collider target in targets)
                {
                    IItemUser itemUser;
                    if (target.TryGetComponent<IItemUser>(out itemUser))
                    {
                        ItemEffect(_Source, itemUser);
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
}