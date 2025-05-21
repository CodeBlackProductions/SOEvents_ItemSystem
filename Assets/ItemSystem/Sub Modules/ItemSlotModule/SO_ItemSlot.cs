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
        private string m_SlotName = "NewSlot";
        private GUID m_SlotGUID;
        private SO_Item m_StoredItem;

        [SerializeField] private SO_Item_Class[] m_AllowedClasses;
        [SerializeField] private SO_Class_Type[] m_AllowedTypes;
        [SerializeField] private SO_Class_Type[] m_AllowedTags;
        [SerializeField] private EAllowedConditions m_AllowConidtions;
        [SerializeField] private SO_Tag[] m_Tags;

        public SO_Item StoredItem
        {
            get => m_StoredItem;
            set
            {
                bool tagMatch = false;
                if (m_AllowedTags?.Length > 0 && value.Tags?.Length > 0)
                {
                    for (int i = 0; i < m_AllowedTags.Length; i++)
                    {
                        for (int o = 0; o < value.Tags.Length; o++)
                        {
                            if (m_AllowedTags[i] == value.Tags[o])
                            {
                                tagMatch = true;
                            }
                        }
                    }
                }
                else if (m_AllowedTags?.Length <= 0)
                {
                    tagMatch = true;
                }

                switch (m_AllowConidtions)
                    {
                        case EAllowedConditions.ClassOrTypeOrTag:
                            if (m_AllowedClasses.Contains(value.Class) || m_AllowedTypes.Contains(value.Class.Types[value.TypeIndex]) || tagMatch)
                            {

                                m_StoredItem = value;
                            }
                            else
                            {
                                Debug.Log("Nope, not allowed in this slot!");
                            }
                            break;

                        case EAllowedConditions.ClassAndTypeAndTag:
                            if (m_AllowedClasses.Contains(value.Class) && m_AllowedTypes.Contains(value.Class.Types[value.TypeIndex]) && tagMatch)
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
        [ItemToolkitAccess] public SO_Tag[] Tags { get => m_Tags; set => m_Tags = value; }

        public string ModuleName { get => m_SlotName; set => m_SlotName = value; }
        public GUID ModuleGUID { get => m_SlotGUID; set => m_SlotGUID = value; }
    }
}