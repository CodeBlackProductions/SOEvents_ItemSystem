using System;
using Unity.VisualScripting;

namespace ItemSystem.SubModules
{
    public class Runtime_Stat
    {
        private object m_Value;
        private System.Type m_Type;

        public Runtime_Stat(object _Value, System.Type _Type)
        {
            m_Value = _Value;
            m_Type = _Type;
        }

        public object Value
        {
            get => m_Value.ConvertTo(m_Type);
            set
            {
                m_Value = value.ConvertTo(m_Type);
            }
        }

        public System.Type Type { get => m_Type; set => m_Type = value; }
    }
}