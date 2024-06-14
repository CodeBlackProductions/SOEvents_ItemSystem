using UnityEngine;

public enum ETarget
{
    Self, Target, TargetsInRangeSelf, TargetsInRangeTarget
}

public abstract class SO_Item_Effect : ScriptableObject
{
    [SerializeField] private string m_EffectName = "NewEffect";
    [SerializeField] private SO_Effect_Trigger m_Trigger;
    [SerializeField] private ETarget m_EffectTarget;

    [ConditionalHide(nameof(m_EffectTarget), 2,3)]
    [SerializeField] private float m_TargetRange = 0;

    public string EffectName { get => m_EffectName; }
    public ETarget EffectTarget { get => m_EffectTarget; }
    public float TargetRange { get => m_TargetRange; }
    public SO_Effect_Trigger Trigger { get => m_Trigger;}

    private void OnEnable()
    {
        m_Trigger?.RegisterEffect(this);
    }

    private void OnDisable()
    {
        m_Trigger?.RemoveEffect(this);
    }

    public void OnInvoke(IItemUser _Source, IItemUser _Target)
    {
        ItemEffect(_Source, _Target);
    }

    protected abstract void ItemEffect(IItemUser _Source, IItemUser _Target);
}