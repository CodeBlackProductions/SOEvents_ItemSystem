using ItemSystem.MainModule;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ItemSystem.Editor;
using System;

namespace ItemSystem.SubModules
{
    public class SO_Stat_Collection : SO_Stat, IItemModule
    {
        [SerializeField] private SO_Stat[] m_Value = new SO_Stat[] { };

        [ItemToolkitAccess] public SO_Stat[] StatValue { get => m_Value; set => m_Value = value; }

        public override System.Type GetStatType()
        {
            return m_Value.GetType();
        }

        public override object GetStatValue()
        {
            return m_Value;
        }

        public override void SetStatValue(object value)
        {
            if (value is SO_Stat[])
            {
                m_Value = (value as SO_Stat[]);
            }
        }
    }
}