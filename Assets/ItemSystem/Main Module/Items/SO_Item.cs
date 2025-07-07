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
        [SerializeField] private List<SO_Stat_Base> m_ItemStats = new List<SO_Stat_Base>();
        [SerializeField] private List<int> m_ItemStatIndices = new List<int>();
        [SerializeField] private int m_TypeIndex;

        private Dictionary<string, SO_Stat_Base> m_Stats = new Dictionary<string, SO_Stat_Base>();
        private Dictionary<string, int> m_StatIndices = new Dictionary<string, int>();

        [ItemToolkitAccess] public string ItemName { get => m_ItemName; set => m_ItemName = value; }
        [ItemToolkitAccess] public SO_Item_Class Class { get => m_Class; set => m_Class = value; }
        [ItemToolkitAccess] public int TypeIndex { get => m_TypeIndex; set => m_TypeIndex = value; }
        [ItemToolkitAccess] public ERarity Rarity { get => m_Rarity; set => m_Rarity = value; }
        [ItemToolkitAccess] public SO_Item_Effect[] Effects { get => m_Effects; set => m_Effects = value; }

        [ItemToolkitAccess]
        public Dictionary<string, SO_Stat_Base> Stats
        {
            get => m_Stats; set
            {
                m_ItemStats.Clear();
                foreach (var stat in value)
                {
                    m_ItemStats.Add(stat.Value);
                    m_ItemStatIndices.Add(m_StatIndices.ContainsKey(stat.Key) ? m_StatIndices[stat.Key] : 0);
                }
                m_Stats = value;
            }
        }

        public Dictionary<string, int> StatIndices { get => m_StatIndices; set => m_StatIndices = value; }
        [ItemToolkitAccess] public SO_ToolTip[] ToolTips { get => m_ToolTips; set => m_ToolTips = value; }
        [ItemToolkitAccess] public SO_Tag[] Tags { get => m_Tags; set => m_Tags = value; }

        public string ModuleName { get => m_ItemName; set => m_ItemName = value; }
        public GUID ModuleGUID { get => m_ItemGUID; set => m_ItemGUID = value; }

        private void OnValidate()
        {
            if (m_ItemStats != null && m_ItemStats.Count > 0)
            {
                m_Stats.Clear();

                for (int i = 0; i < m_ItemStats.Count; i++)
                {
                    var stat = m_ItemStats[i];
                    int index = i < m_ItemStatIndices.Count ? m_ItemStatIndices[i] : 0;
                    if (stat != null && !m_Stats.ContainsKey(stat.TargetUserStat))
                    {
                        m_Stats.Add(stat.TargetUserStat, stat);
                        m_StatIndices.Add(stat.TargetUserStat, index);
                    }
                }
            }
        }
    }
}