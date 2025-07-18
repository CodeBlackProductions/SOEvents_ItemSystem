using ItemSystem.Editor;
using ItemSystem.SubModules;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ItemSystem.MainModule
{
    /// <summary>
    /// Base item type, subcategory of <see cref="SO_Item_Class"/> (e.g. weapon / one handed weapon).
    /// Inherit from this when creating new item types.
    /// </summary>
    public abstract class SO_Class_Type : ScriptableObject, IItemModule, IItemModuleBase
    {
        [SerializeField] private string m_TypeName = "NewType";
        [SerializeField] private GUID m_TypeGUID;
        [SerializeField] private SO_ToolTip[] m_ToolTips;
        [SerializeField] private List<SO_Stat> m_TypeStats = new List<SO_Stat>();
        [SerializeField] private List<int> m_TypeStatIndices = new List<int>();
        [SerializeField] private SO_Tag[] m_Tags;

        private Dictionary<string, SO_Stat> m_Stats = new Dictionary<string, SO_Stat>();
        private Dictionary<string, int> m_StatIndices = new Dictionary<string, int>();

        [ItemToolkitAccess] public string TypeName { get => m_TypeName; set => m_TypeName = value; }

        [ItemToolkitAccess]
        public Dictionary<string, SO_Stat> Stats
        {
            get => m_Stats; set
            {
                m_TypeStats.Clear();
                m_TypeStatIndices.Clear();
                foreach (var stat in value)
                {
                    m_TypeStats.Add(stat.Value);
                    m_TypeStatIndices.Add(m_StatIndices.ContainsKey(stat.Key) ? m_StatIndices[stat.Key] : 0);
                }
                m_Stats = value;
            }
        }

        public Dictionary<string, int> StatIndices { get => m_StatIndices; set => m_StatIndices = value; }

        [ItemToolkitAccess] public SO_ToolTip[] ToolTips { get => m_ToolTips; set => m_ToolTips = value; }
        [ItemToolkitAccess] public SO_Tag[] Tags { get => m_Tags; set => m_Tags = value; }

        public string ModuleName { get => m_TypeName; set => m_TypeName = value; }
        public GUID ModuleGUID { get => m_TypeGUID; set => m_TypeGUID = value; }

        private void OnValidate()
        {
            if (m_TypeStats != null && m_TypeStats.Count > 0)
            {
                m_Stats.Clear();
                m_StatIndices.Clear();
                for (int i = 0; i < m_TypeStats.Count; i++)
                {
                    var stat = m_TypeStats[i];
                    int index = i < m_TypeStatIndices.Count ? m_TypeStatIndices[i] : 0;
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