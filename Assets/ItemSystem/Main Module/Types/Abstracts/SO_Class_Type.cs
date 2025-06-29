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
        [SerializeField] private SO_Tag[] m_Tags;

        private Dictionary<string, SO_Stat> m_Stats = new Dictionary<string, SO_Stat>();

        [ItemToolkitAccess] public string TypeName { get => m_TypeName; set => m_TypeName = value; }

        [ItemToolkitAccess]
        public Dictionary<string, SO_Stat> Stats
        {
            get => m_Stats; set
            {
                m_TypeStats.Clear();
                foreach (var stat in value)
                {
                    m_TypeStats.Add(stat.Value);
                }
                m_Stats = value;
            }
        }

        [ItemToolkitAccess] public SO_ToolTip[] ToolTips { get => m_ToolTips; set => m_ToolTips = value; }
        [ItemToolkitAccess] public SO_Tag[] Tags { get => m_Tags; set => m_Tags = value; }

        public string ModuleName { get => m_TypeName; set => m_TypeName = value; }
        public GUID ModuleGUID { get => m_TypeGUID; set => m_TypeGUID = value; }

        private void OnValidate()
        {
            if (m_TypeStats != null && m_TypeStats.Count > 0)
            {
                m_Stats.Clear();
                foreach (SO_Stat stat in m_TypeStats)
                {
                    if (stat != null && !m_Stats.ContainsKey(stat.TargetUserStat))
                    {
                        m_Stats.Add(stat.TargetUserStat, stat);
                    }
                }
            }
        }
    }
}