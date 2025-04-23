using System;
using Unity.VisualScripting;

namespace ItemSystem.SubModules
{
    public class Runtime_Stat
    {
        private object m_Value;
        private Type m_Type;

        public Runtime_Stat(object _Value, Type _Type)
        {
            m_Value = _Value;
            m_Type = _Type;
        }

        public object Value
        {
            get => m_Value.ConvertTo(m_Type);
            set
            {
                if (m_Type == value.GetType())
                {
                    m_Value = value;
                }
            }
        }

        public Type Type { get => m_Type; set => m_Type = value; }
    }
}