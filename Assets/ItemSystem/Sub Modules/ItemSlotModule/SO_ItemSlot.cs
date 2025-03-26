using System.Linq;
using UnityEditor;
using UnityEngine;

public enum EAllowedConditions
{
    ClassOrType, ClassAndType
}

/// <summary>
/// Optional item slots for usage in an inventory system.
/// </summary>
public class SO_ItemSlot : ScriptableObject, IItemModule
{
    private string m_SlotName = "NewSlot";
    private GUID m_SlotGUID;
    private SO_Item m_StoredItem;

    [SerializeField] private SO_Item_Class[] m_AllowedClasses;
    [SerializeField] private SO_Class_Type[] m_AllowedTypes;
    [SerializeField] private EAllowedConditions m_AllowConidtions;

    public SO_Item StoredItem
    {
        get => m_StoredItem;
        set
        {
            switch (m_AllowConidtions)
            {
                case EAllowedConditions.ClassOrType:
                    if (m_AllowedClasses.Contains(value.Class) || m_AllowedTypes.Contains(value.Class.Types[value.TypeIndex]))
                    {
                        m_StoredItem = value;
                    }
                    else
                    {
                        Debug.Log("Nope, not allowed in this slot!");
                    }
                    break;

                case EAllowedConditions.ClassAndType:
                    if (m_AllowedClasses.Contains(value.Class) && m_AllowedTypes.Contains(value.Class.Types[value.TypeIndex]))
                    {
                        m_StoredItem = value;
                    }
                    else
                    {
                        Debug.Log("Nope, not allowed in this slot!");
                    }
                    break;

                default:
                    break;
            }
        }
    }

    [ItemToolkitAccess] public string SlotName { get => m_SlotName; set => m_SlotName = value; }
    [ItemToolkitAccess] public SO_Item_Class[] AllowedClasses { get => m_AllowedClasses; set => m_AllowedClasses = value; }
    [ItemToolkitAccess] public SO_Class_Type[] AllowedTypes { get => m_AllowedTypes; set => m_AllowedTypes = value; }
    [ItemToolkitAccess] public EAllowedConditions AllowConidtions { get => m_AllowConidtions; set => m_AllowConidtions = value; }

    public string ModuleName { get => m_SlotName; set => m_SlotName = value; }
    public GUID ModuleGUID { get => m_SlotGUID; set => m_SlotGUID = value; }
}