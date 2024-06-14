using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New_ItemSlot", menuName = "ItemSystem/ItemSlot")]
public class SO_ItemSlot : ScriptableObject
{
    private SO_Item m_StoredItem;

    [SerializeField] private SO_Item_Class[] m_AllowedClasses;
    [SerializeField] private SO_Class_Type[] m_AllowedTypes;

    public SO_Item StoredItem
    {        get => m_StoredItem;
        set
        {
            if (m_AllowedClasses.Contains(value.Class) || m_AllowedTypes.Contains(value.Class.Type))
            {
                m_StoredItem = value;
            }
            else
            {
                Debug.Log("Nope, not allowed in this slot!");
            }
        }
    }
}