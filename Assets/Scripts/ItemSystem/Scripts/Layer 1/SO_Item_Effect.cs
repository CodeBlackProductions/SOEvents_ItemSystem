using UnityEngine;

public enum ETarget
{
    Self, Target, Range
}

[CreateAssetMenu(fileName = "New_Effect", menuName = "ItemSystem/Effect/Effect")]
public class SO_Item_Effect : ScriptableObject
{
    [SerializeField] private string m_EffectName;
    [SerializeField] private SO_Effect_Trigger m_Trigger;
    [SerializeField] private ETarget m_EffectTarget;
    [SerializeField] private float m_Range;

    public string EffectName { get => m_EffectName; }
    public ETarget EffectTarget { get => m_EffectTarget; }
    public float Range { get => m_Range; }
}