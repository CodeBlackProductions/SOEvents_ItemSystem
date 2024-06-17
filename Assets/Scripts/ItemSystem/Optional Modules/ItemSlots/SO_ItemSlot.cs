using System.Linq;
using UnityEngine;


enum Allowed
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
    [SerializeField] private Allowed m_AllowConidtions;
    public SO_Item StoredItem
    {        get => m_StoredItem;
        set
        {
            switch (m_AllowConidtions)
            {
                case Allowed.ClassOrType:
                    if (m_AllowedClasses.Contains(value.Class) || m_AllowedTypes.Contains(value.Class.Type))
                    {
                        m_StoredItem = value;
                    }
                    else
                    {
                        Debug.Log("Nope, not allowed in this slot!");
                    }
                    break;
                case Allowed.ClassAndType:
                    if (m_AllowedClasses.Contains(value.Class) && m_AllowedTypes.Contains(value.Class.Type))
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