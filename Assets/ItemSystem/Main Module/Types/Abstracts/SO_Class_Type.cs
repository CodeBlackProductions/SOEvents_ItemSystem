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
    public abstract class SO_Class_Type : ScriptableObject, IItemModule
    {
        [SerializeField] private string m_TypeName = "NewType";
        [SerializeField] private GUID m_TypeGUID;
        [SerializeField] private List<SO_Stat> m_TypeStats;

        private Dictionary<string, SO_Stat> m_Stats = new Dictionary<string, SO_Stat>();

        [ItemToolkitAccess] public string TypeName { get => m_TypeName; set => m_TypeName = value; }
        [ItemToolkitAccess] public Dictionary<string, SO_Stat> Stats { get => m_Stats; set => m_Stats = value; }

        public string ModuleName { get => m_TypeName; set => m_TypeName = value; }
        public GUID ModuleGUID { get => m_TypeGUID; set => m_TypeGUID = value; }

        private void OnValidate()
        {
            if (m_TypeStats != null && m_TypeStats.Count > 0)
            {
                m_Stats.Clear();
                foreach (SO_Stat stat in m_TypeStats)
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