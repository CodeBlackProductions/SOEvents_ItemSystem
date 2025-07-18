using ItemSystem.Editor;
using ItemSystem.MainModule;
using System.Collections.Generic;
using UnityEngine;

namespace ItemSystem.SubModules
{
    public class SO_Stat_Collection : SO_Stat_StaticValue, IItemModule
    {
        [SerializeField] private List<SO_Stat> m_ValueSerialized = new List<SO_Stat>();
        [SerializeField] private List<int> m_IndicesSerialized =new List<int>();

        private Dictionary<string, SO_Stat> m_Value = new Dictionary<string, SO_Stat>();
        private Dictionary<string, int> m_Indices = new Dictionary<string, int>();

        [ItemToolkitAccess]
        public Dictionary<string, SO_Stat> Stats
        {
            get => m_Value; set
            {
                m_ValueSerialized.Clear();
                m_IndicesSerialized.Clear();
                foreach (var stat in value)
                {
                    m_ValueSerialized.Add(stat.Value);
                    m_IndicesSerialized.Add(m_Indices.ContainsKey(stat.Key) ? m_Indices[stat.Key] : 0);
                }
                m_Value = value;
            }
        }

        public Dictionary<string, int> StatIndices { get => m_Indices; set => m_Indices = value; }

        public override System.Type GetStatType()
        {
            return m_Value.GetType();
        }

        public override object GetStatValue()
        {
            return m_Value;
        }

        public object GetStatIndices()
        {
            return m_Indices;
        }

        public override void SetStatValue(object _Value)
        {
            if (_Value is SO_Stat[])
            {
                m_Value = (_Value as Dictionary<string, SO_Stat>);
            }
        }

        public void SetStatIndices(object _Value)
        {
            m_Indices = _Value as Dictionary<string,int>;
        }

        private void OnValidate()
        {
            if (m_ValueSerialized != null && m_ValueSerialized.Count > 0)
            {
                m_Value.Clear();
                m_Indices.Clear();
                for (int i = 0; i < m_ValueSerialized.Count; i++)
                {
                    var stat = m_ValueSerialized[i];
                    int index = i < m_IndicesSerialized.Count ? m_IndicesSerialized[i] : 0;
                    if (stat != null && !m_Value.ContainsKey(stat.TargetUserStat))
                    {
                        m_Value.Add(stat.TargetUserStat, stat);
                        m_Indices.Add(stat.TargetUserStat, index);
                    }
                }
            }
        }
    }
}