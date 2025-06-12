using ItemSystem.Editor;
using ItemSystem.SubModules;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ItemSystem.MainModule
{
    public enum ERarity
    {
        Common, Uncommon, Rare, Legendary, Unique
    }

    /// <summary>
    /// Core class of the entire system. Contains all necessary pieces to create a functioning item.
    /// </summary>

    public class SO_Item : ScriptableObject, IItemModule, IEffectModule, IItemModuleBase
    {
        [SerializeField] private string m_ItemName = "NewItem";
        [SerializeField] private GUID m_ItemGUID;
        [SerializeField] private SO_ToolTip[] m_ToolTips;
        [SerializeField] private SO_Item_Class m_Class;
        [SerializeField] private ERarity m_Rarity;
        [SerializeField] private SO_Item_Effect[] m_Effects;
        [SerializeField] private SO_Tag[] m_Tags;
        [SerializeField] private List<SO_Stat> m_ItemStats = new List<SO_Stat>();
        [SerializeField] private int m_TypeIndex;

        private Dictionary<string, SO_Stat> m_Stats = new Dictionary<string, SO_Stat>();

        [ItemToolkitAccess] public string ItemName { get => m_ItemName; set => m_ItemName = value; }
        [ItemToolkitAccess] public SO_Item_Class Class { get => m_Class; set => m_Class = value; }
        [ItemToolkitAccess] public int TypeIndex { get => m_TypeIndex; set => m_TypeIndex = value; }
        [ItemToolkitAccess] public ERarity Rarity { get => m_Rarity; set => m_Rarity = value; }
        [ItemToolkitAccess] public SO_Item_Effect[] Effects { get => m_Effects; set => m_Effects = value; }

        [ItemToolkitAccess]
        public Dictionary<string, SO_Stat> Stats
        {
            get => m_Stats; set
            {
                m_ItemStats.Clear();
                foreach (var stat in value)
                {
                    m_ItemStats.Add(stat.Value);
                }
                m_Stats = value;
            }
        }

        [ItemToolkitAccess] public SO_ToolTip[] ToolTips { get => m_ToolTips; set => m_ToolTips = value; }
        [ItemToolkitAccess] public SO_Tag[] Tags { get => m_Tags; set => m_Tags = value; }

        public string ModuleName { get => m_ItemName; set => m_ItemName = value; }
        public GUID ModuleGUID { get => m_ItemGUID; set => m_ItemGUID = value; }

        private void OnValidate()
        {
            if (m_ItemStats != null && m_ItemStats.Count > 0)
            {
                m_Stats.Clear();
                foreach (SO_Stat stat in m_ItemStats)
                {
                    if (stat != null && !m_Stats.ContainsKey(stat.StatName))
                    {
                        m_Stats.Add(stat.StatName, stat);
                    }
                }
            }
        }
    }
}