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
            foreach (var stat in _User.Stats)
            {
                switch (stat)
                {
                    case SO_Stat_Collection collection:
                        Dictionary<string, SO_Stat> collectionContent = collection.GetStatValue() as Dictionary<string, SO_Stat>;
                        Dictionary<string, int> collectionIndices = collection.GetStatIndices() as Dictionary<string, int>;

                        foreach (var (subKey, subStat) in collectionContent)
                        {
                            if (subStat is SO_Stat_DynamicValue dyn)
                            {
                                RegisterStat(_User, dyn, collectionIndices.TryGetValue(subKey, out var subIndex) ? subIndex : 0);
                            }
                            else if (subStat is SO_Stat_StaticValue staticVal)
                            {
                                RegisterStat(_User, staticVal);
                            }
                        }
                        break;

                    case SO_Stat_StaticValue staticValue:
                        RegisterStat(_User, staticValue);
                        break;

                    case SO_Stat_DynamicValue dynamicStat:
                        Debug.LogWarning("StatLoader: Non-static stats should not directly be added to the user as you can not define a specifix index this way. Registered index 0 as default value.");
                        RegisterStat(_User, dynamicStat, 0);
                        break;
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

    private void RegisterStat(IItemUser _User, SO_Stat_DynamicValue _Stat, int _Index, bool _OverrideMode)
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

    private void RegisterStat(IItemUser _User, SO_Stat_DynamicValue _Stat, int _Index)
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
                        IterateOverStats(_User, itemStats, item);

                    if (classStats != null && classStats.Count > 0)
                        IterateOverStats(_User, classStats, item.Class);

                    if (typeStats != null && typeStats.Count > 0)
                        IterateOverStats(_User, typeStats, item.Class.Types[item.TypeIndex]);
                }
                else
                {
                    if (typeStats != null && typeStats.Count > 0)
                        IterateOverStats(_User, typeStats, item.Class.Types[item.TypeIndex]);

                    if (classStats != null && classStats.Count > 0)
                        IterateOverStats(_User, classStats, item.Class);

                    if (itemStats != null && itemStats.Count > 0)
                        IterateOverStats(_User, itemStats, item);
                }
            }
        }
    }

    private void IterateOverStats(IItemUser _User, Dictionary<string, SO_Stat> _Collection, object _Source)
    {
        Dictionary<string, int> indices = _Source switch
        {
            SO_Item item => item.StatIndices,
            SO_Item_Class itemClass => itemClass.StatIndices,
            SO_Class_Type classType => classType.StatIndices,
            _ => null
        };

        foreach (var (key, stat) in _Collection)
        {
            switch (stat)
            {
                case SO_Stat_Collection collection:
                    Dictionary<string, SO_Stat> collectionContent = collection.GetStatValue() as Dictionary<string, SO_Stat>;
                    Dictionary<string, int> collectionIndices = collection.GetStatIndices() as Dictionary<string, int>;

                    foreach (var (subKey, subStat) in collectionContent)
                    {
                        if (subStat is SO_Stat_DynamicValue dyn)
                        {
                            RegisterStat(_User, dyn, collectionIndices.TryGetValue(subKey, out var subIndex) ? subIndex : 0);
                        }
                        else if (subStat is SO_Stat_StaticValue staticVal)
                        {
                            RegisterStat(_User, staticVal);
                        }
                    }
                    break;

                case SO_Stat_StaticValue staticValue:
                    RegisterStat(_User, staticValue);
                    break;

                case SO_Stat_DynamicValue dynamicStat:
                    int index = indices?.TryGetValue(key, out var i) == true ? i : 0;
                    RegisterStat(_User, dynamicStat, index);
                    break;
            }
        }
    }
}