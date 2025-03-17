using System.Linq;
using UnityEngine;


enum EAllowedConditions
{
    ClassOrType, ClassAndType
}

/// <summary>
/// Optional item slots for usage in an inventory system.
/// </summary>
[CreateAssetMenu(fileName = "New_ItemSlot", menuName = "ItemSystem/ItemSlot")]
public class SO_ItemSlot : ScriptableObject
{
    private SO_Item m_StoredItem;

    [SerializeField] private SO_Item_Class[] m_AllowedClasses;
    [SerializeField] private SO_Class_Type[] m_AllowedTypes;
    [SerializeField] private EAllowedConditions m_AllowConidtions;
    public SO_Item StoredItem
    {        get => m_StoredItem;
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
}