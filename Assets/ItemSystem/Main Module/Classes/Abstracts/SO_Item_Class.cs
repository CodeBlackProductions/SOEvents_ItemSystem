using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ItemSystem.Editor;
using ItemSystem.SubModules;

namespace ItemSystem.MainModule
{
    /// <summary>
    /// Base item class, subcategory of <see cref="SO_Item"/> (e.g. Sword / weapon).
    /// Inherit from this when creating new item classes.
    /// </summary>
    public abstract class SO_Item_Class : ScriptableObject, IItemModule, IItemModuleBase
    {
        [SerializeField] private string m_ClassName = "NewClass";
        [SerializeField] private GUID m_ClassGUID;
        [SerializeField] private SO_ToolTip[] m_ToolTips;
        [SerializeField] private SO_Class_Type[] m_Types;
        [SerializeField] private List<SO_Stat_Base> m_ClassStats = new List<SO_Stat_Base>();
        [SerializeField] private List<int> m_ClassStatIndices = new List<int>();
        [SerializeField] private SO_Tag[] m_Tags;

        private Dictionary<string, SO_Stat_Base> m_Stats = new Dictionary<string, SO_Stat_Base>();
        private Dictionary<string, int> m_StatIndices = new Dictionary<string, int>();

        [ItemToolkitAccess] public string ClassName { get => m_ClassName; set => m_ClassName = value; }
        [ItemToolkitAccess] public SO_Class_Type[] Types { get => m_Types; set => m_Types = value; }

        [ItemToolkitAccess]
        public Dictionary<string, SO_Stat_Base> Stats
        {
            get => m_Stats; set
            {
                m_ClassStats.Clear();
                foreach (var stat in value)
                {
                    m_ClassStats.Add(stat.Value);
                    m_ClassStatIndices.Add(m_StatIndices.ContainsKey(stat.Key) ? m_StatIndices[stat.Key] : 0);
                }
                m_Stats = value;
            }
        }

        public Dictionary<string, int> StatIndices { get => m_StatIndices; set => m_StatIndices = value; }

        [ItemToolkitAccess] public SO_ToolTip[] ToolTips { get => m_ToolTips; set => m_ToolTips = value; }

        [ItemToolkitAccess] public SO_Tag[] Tags { get => m_Tags; set => m_Tags = value; }

        public string ModuleName { get => m_ClassName; set => m_ClassName = value; }
        public GUID ModuleGUID { get => m_ClassGUID; set => m_ClassGUID = value; }

        private void OnValidate()
        {
            if (m_ClassStats != null && m_ClassStats.Count > 0)
            {
                m_Stats.Clear();

                for (int i = 0; i < m_ClassStats.Count; i++)
                {
                    var stat = m_ClassStats[i];
                    int index = i < m_ClassStatIndices.Count ? m_ClassStatIndices[i] : 0;
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