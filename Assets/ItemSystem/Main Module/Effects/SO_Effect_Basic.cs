using ItemSystem.Editor;
using ItemSystem.MainModule;
using ItemSystem.SubModules;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class SO_Effect_Basic : SO_Item_Effect
{
    [SerializeField] private SerializableKeyValuePair<SO_Stat, int> m_SingularStatExample_FloatOnly;
    [SerializeField] private SerializableKeyValuePair<SO_Stat, int> m_SingularStatExample_StringAndArmorTypeOnly;
    [SerializeField] private SerializableKeyValuePair<SO_Stat, int> m_SingularStatExample_FloatOrArmorTypeOnly;

    [ItemToolkitAccess]
    [ItemToolkitFilter(typeof(SO_Stat_Float))]
    public SerializableKeyValuePair<SO_Stat, int> SingularStatExample_FloatOnly { get => m_SingularStatExample_FloatOnly; set => m_SingularStatExample_FloatOnly = value; }

    [ItemToolkitAccess]
    [ItemToolkitFilter(true, typeof(SO_Stat_String))]
    [ItemToolkitFilter(true, "ArmorType")]
    public SerializableKeyValuePair<SO_Stat, int> SingularStatExample_StringAndArmorTypeOnly { get => m_SingularStatExample_StringAndArmorTypeOnly; set => m_SingularStatExample_StringAndArmorTypeOnly = value; }

    [ItemToolkitAccess]
    [ItemToolkitFilter(typeof(SO_Stat_Float))]
    [ItemToolkitFilter("ArmorType")]
    public SerializableKeyValuePair<SO_Stat, int> SingularStatExample_FloatOrArmorTypeOnly { get => m_SingularStatExample_FloatOrArmorTypeOnly; set => m_SingularStatExample_FloatOrArmorTypeOnly = value; }

    protected override void ItemEffect(IItemUser _Source, IItemUser _Target)
    {
        Debug.Log("This is just a " + EffectName + " Source: " + _Source + " Target: " + _Target);
    }
}