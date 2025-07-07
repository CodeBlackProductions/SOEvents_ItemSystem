using ItemSystem.MainModule;
using ItemSystem.SubModules;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SO_StatLoader : ScriptableObject
{
    public enum EStatLoadOrder
    {
        HighToLow, LowToHigh
    }

    public enum ENonAddableStatBehaviour
    {
        KeepFirst, KeepLast
    }

    public enum EBaseStatBehaviour
    {
        Apply, Ignore, Override, Exclusive
    }

    [SerializeField] private EStatLoadOrder m_LoadOrder = EStatLoadOrder.LowToHigh;
    [SerializeField] private ENonAddableStatBehaviour m_OverrideBehaviour = ENonAddableStatBehaviour.KeepLast;
    [SerializeField] private EBaseStatBehaviour m_BaseStatBehaviour = EBaseStatBehaviour.Apply;

    public EStatLoadOrder LoadOrder { get => m_LoadOrder; set => m_LoadOrder = value; }
    public ENonAddableStatBehaviour OverrideBehaviour { get => m_OverrideBehaviour; set => m_OverrideBehaviour = value; }
    public EBaseStatBehaviour BaseStatBehaviour { get => m_BaseStatBehaviour; set => m_BaseStatBehaviour = value; }

    public void LoadStats(IItemUser _User)
    {
        switch (m_BaseStatBehaviour)
        {
            case EBaseStatBehaviour.Ignore:
                LoadItemStats(_User, m_LoadOrder == EStatLoadOrder.HighToLow);
                break;

            case EBaseStatBehaviour.Override:
                LoadBaseStats(_User, true);
                LoadItemStats(_User, m_LoadOrder == EStatLoadOrder.HighToLow);
                break;

            case EBaseStatBehaviour.Exclusive:
                LoadBaseStats(_User, false);
                break;

            case EBaseStatBehaviour.Apply:
            default:
                if (m_LoadOrder == EStatLoadOrder.HighToLow)
                {
                    LoadItemStats(_User, true);
                    LoadBaseStats(_User, false);
                }
                else
                {
                    LoadBaseStats(_User, false);
                    LoadItemStats(_User, false);
                }
                break;
        }
    }

    private void LoadBaseStats(IItemUser _User, bool _OverrideMode)
    {
        if (_User.Stats != null && _User.Stats.Count > 0)
        {
            for (int i = 0; i < _User.Stats.Count; i++)
            {
                if (_User.Stats[i] is SO_Stat_Collection collection)
                {
                    SO_Stat_StaticValue[] collectionContent = collection.GetStatValue() as SO_Stat_StaticValue[];
                    for (int c = 0; c < collectionContent.Length; c++)
                    {
                        RegisterStat(_User, collectionContent[c], _OverrideMode);
                    }
                }
                else if (_User.Stats[i] is SO_Stat_StaticValue staticValue)
                {
                    RegisterStat(_User, staticValue, _OverrideMode);
                }
                else if (_User.Stats[i] is SO_Stat value)
                {
                    RegisterStat(_User, value, 0, _OverrideMode);
                }
            }
        }
    }

    private void RegisterStat(IItemUser _User, SO_Stat_StaticValue _Stat, bool _OverrideMode)
    {
        if (!_User.UserStats.ContainsKey(_Stat.TargetUserStat))
        {
            _User.UserStats.Add(_Stat.TargetUserStat, new Runtime_Stat(_Stat.GetStatValue(), _Stat.GetStatType()));
        }
        else if (_Stat.GetStatType().IsNumeric() && _User.UserStats.ContainsKey(_Stat.TargetUserStat))
        {
            if (_OverrideMode)
            {
                _User.UserStats[_Stat.TargetUserStat].Value = _Stat.GetStatValue();
            }
            else
            {
                _User.UserStats[_Stat.TargetUserStat].Value =
                    (double.Parse(_User.UserStats[_Stat.TargetUserStat].Value.ToString()) +
                     double.Parse(_Stat.GetStatValue().ToString()));
            }
        }
        else
        {
            if (m_OverrideBehaviour == ENonAddableStatBehaviour.KeepFirst && !_OverrideMode) return;
            _User.UserStats[_Stat.TargetUserStat].Value = _Stat.GetStatValue();
        }
    }

    private void RegisterStat(IItemUser _User, SO_Stat_StaticValue _Stat)
    {
        RegisterStat(_User, _Stat, false);
    }

    private void RegisterStat(IItemUser _User, SO_Stat _Stat, int _Index, bool _OverrideMode)
    {
        if (!_User.UserStats.ContainsKey(_Stat.TargetUserStat))
        {
            _User.UserStats.Add(_Stat.TargetUserStat, new Runtime_Stat(_Stat.GetStatValue(_Index), _Stat.GetStatType()));
        }
        else if (_Stat.GetStatType().IsNumeric() && _User.UserStats.ContainsKey(_Stat.TargetUserStat))
        {
            if (_OverrideMode)
            {
                _User.UserStats[_Stat.TargetUserStat].Value = _Stat.GetStatValue(_Index);
            }
            else
            {
                _User.UserStats[_Stat.TargetUserStat].Value =
                    (double.Parse(_User.UserStats[_Stat.TargetUserStat].Value.ToString()) +
                     double.Parse(_Stat.GetStatValue(_Index).ToString()));
            }
        }
        else
        {
            if (m_OverrideBehaviour == ENonAddableStatBehaviour.KeepFirst && !_OverrideMode) return;
            _User.UserStats[_Stat.TargetUserStat].Value = _Stat.GetStatValue(_Index);
        }
    }

    private void RegisterStat(IItemUser _User, SO_Stat _Stat, int _Index)
    {
        RegisterStat(_User, _Stat, _Index, false);
    }

    private void LoadItemStats(IItemUser _User, bool _InvertOrder)
    {
        if (_User.Items != null && _User.Items.Count > 0)
        {
            for (int i = 0; i < _User.Items.Count; i++)
            {
                var item = _User.Items[i];
                var typeStats = item.Class.Types[item.TypeIndex].Stats;
                var classStats = item.Class.Stats;
                var itemStats = item.Stats;

                if (_InvertOrder)
                {
                    if (itemStats != null && itemStats.Count > 0)
                        IterateOverStats(_User, itemStats);

                    if (classStats != null && classStats.Count > 0)
                        IterateOverStats(_User, classStats);

                    if (typeStats != null && typeStats.Count > 0)
                        IterateOverStats(_User, typeStats);
                }
                else
                {
                    if (typeStats != null && typeStats.Count > 0)
                        IterateOverStats(_User, typeStats);

                    if (classStats != null && classStats.Count > 0)
                        IterateOverStats(_User, classStats);

                    if (itemStats != null && itemStats.Count > 0)
                        IterateOverStats(_User, itemStats);
                }
            }
        }
    }

    private void IterateOverStats(IItemUser _User, Dictionary<string, SO_Stat_Base> _Collection)
    {
        foreach (var stat in _Collection)
        {
            if (stat.Value is SO_Stat_Collection collection)
            {
                SO_Stat_StaticValue[] collectionContent = collection.GetStatValue() as SO_Stat_StaticValue[];
                for (int c = 0; c < collectionContent.Length; c++)
                {
                    RegisterStat(_User, collectionContent[c]);
                }
            }
            else if (stat.Value is SO_Stat_StaticValue staticValue)
            {
                RegisterStat(_User, staticValue);
            }
            else if (stat.Value is SO_Stat value)
            {
                Debug.LogWarning("StatLoader: Still missing a way to determine correct Stat from non static stats!");
                RegisterStat(_User, value, 0);
            }
        }
    }
}