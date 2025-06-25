using ItemSystem.Editor;
using ItemSystem.MainModule;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ItemSystem.SubModules
{
    public enum EAllowedConditions
    {
        ClassOrTypeOrTag, ClassAndTypeAndTag
    }

    /// <summary>
    /// Optional item slots for usage in an inventory system.
    /// </summary>
    public class SO_ItemSlot : ScriptableObject, IItemModule, IItemModuleBase
    {
        private GUID m_SlotGUID;
        private SO_Item m_StoredItem;

        [SerializeField] private string m_SlotName = "NewSlot";
        [SerializeField] private SO_Item_Class[] m_AllowedClasses;
        [SerializeField] private SO_Class_Type[] m_AllowedTypes;
        [SerializeField] private SO_Tag[] m_AllowedTags;
        [SerializeField] private EAllowedConditions m_AllowConidtions;
        [SerializeField] private SO_ToolTip[] m_ToolTips;
        [SerializeField] private SO_Tag[] m_Tags;

        private bool m_TagMatch = false;
        private bool m_TypeMatch = false;
        private bool m_ClassMatch = false;

        public SO_Item StoredItem
        {
            get => m_StoredItem;
            set
            {
                if (value == null)
                {
                    m_StoredItem = null;
                    return;
                }

                m_TagMatch = false;
                m_TypeMatch = false;
                m_ClassMatch = false;

                CheckConditions(value);

                switch (m_AllowConidtions)
                {
                    case EAllowedConditions.ClassOrTypeOrTag:
                        if (m_ClassMatch || m_TypeMatch || m_TagMatch)
                        {
                            m_StoredItem = value;
                        }
                        else
                        {
                            Debug.Log($"Nope, {value.ItemName} is not allowed in this slot!");
                        }
                        break;

                    case EAllowedConditions.ClassAndTypeAndTag:

                        if (m_ClassMatch && m_TypeMatch && m_TagMatch)
                        {
                            m_StoredItem = value;
                        }
                        else
                        {
                            Debug.Log($"Nope, {value.ItemName} is not allowed in this slot!");
                        }
                        break;

                    default:
                        Debug.Log($"Nope, {value.ItemName} is not allowed in this slot!");
                        break;
                }
            }
        }

        [ItemToolkitAccess] public string SlotName { get => m_SlotName; set => m_SlotName = value; }
        [ItemToolkitAccess] public SO_Item_Class[] AllowedClasses { get => m_AllowedClasses; set => m_AllowedClasses = value; }
        [ItemToolkitAccess] public SO_Class_Type[] AllowedTypes { get => m_AllowedTypes; set => m_AllowedTypes = value; }
        [ItemToolkitAccess] public SO_Tag[] AllowedTags { get => m_AllowedTags; set => m_AllowedTags = value; }
        [ItemToolkitAccess] public EAllowedConditions AllowConidtions { get => m_AllowConidtions; set => m_AllowConidtions = value; }
        [ItemToolkitAccess] public SO_ToolTip[] ToolTips { get => m_ToolTips; set => m_ToolTips = value; }
        [ItemToolkitAccess] public SO_Tag[] Tags { get => m_Tags; set => m_Tags = value; }

        public string ModuleName { get => m_SlotName; set => m_SlotName = value; }
        public GUID ModuleGUID { get => m_SlotGUID; set => m_SlotGUID = value; }

        private void CheckConditions(SO_Item _Item)
        {
            if (m_AllowedClasses?.Length > 0)
            {
                m_ClassMatch = m_AllowedClasses.Contains(_Item.Class);
            }
            else
            {
                m_ClassMatch = true;
            }
            if (m_AllowedTypes?.Length > 0)
            {
                m_TypeMatch = m_AllowedTypes.Contains(_Item.Class.Types[_Item.TypeIndex]);
            }
            else
            {
                m_TypeMatch = true;
            }
            if (m_AllowedTags?.Length > 0)
            {
                foreach (var tag in _Item.Tags)
                {
                    if (m_AllowedTags.Contains(tag))
                    {
                        m_TagMatch = true;
                    }
                }
            }
            else
            {
                m_TagMatch = true;
            }
        }
    }
}