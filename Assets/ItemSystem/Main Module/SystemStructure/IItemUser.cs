using ItemSystem.SubModules;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

namespace ItemSystem.MainModule
{
    /// <summary>
    /// Adds item system functionality to a custom unit/character script.
    /// </summary>
    public interface IItemUser
    {
        public List<SO_Item> Items { get; set; }

        public List<SO_Stat> Stats { get; set; }

        public GameObject ImplementingUser { get; }
        public int Team { get; }

        public Dictionary<string, Runtime_Stat> UserStats { get; }
        public Dictionary<SO_Effect_Trigger, List<SO_Item_Effect>> EffectRegistry { get; }

        /// <summary>
        /// Initializes the item effects and stats into the registry. Needed for checking if an effect has to be triggered later on.
        /// </summary>
        public void OnInitialize()
        {
            if (Stats != null && Stats.Count > 0)
            {
                for (int i = 0; i < Stats.Count; i++)
                {
                    RegisterStat(Stats[i]);
                }
            }

            if (Items != null && Items.Count > 0)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    EffectRegistryHelper.RegisterAllEffectsRecursive(Items[i], effect =>
                    {
                        if (effect != null)
                        {
                            if (EffectRegistry.ContainsKey(effect.Trigger))
                            {
                                EffectRegistry[effect.Trigger].Add(effect);
                            }
                            else
                            {
                                EffectRegistry.Add(effect.Trigger, new List<SO_Item_Effect> { effect });
                            }
                        }
                    });

                    //for (int e = 0; e < Items[i].Effects.Length; e++)
                    //{
                    //    if (EffectRegistry.ContainsKey(Items[i].Effects[e].Trigger))
                    //    {
                    //        EffectRegistry[Items[i].Effects[e].Trigger].Add(Items[i].Effects[e]);
                    //    }
                    //    else
                    //    {
                    //        EffectRegistry.Add(Items[i].Effects[e].Trigger, new List<SO_Item_Effect>());
                    //        EffectRegistry[Items[i].Effects[e].Trigger].Add(Items[i].Effects[e]);
                    //    }
                    //}

                    if (Items[i].Class.Types[Items[i].TypeIndex].Stats != null && Items[i].Class.Types[Items[i].TypeIndex].Stats.Count > 0)
                    {
                        foreach (var stat in Items[i].Class.Types[Items[i].TypeIndex].Stats)
                        {
                            RegisterStat(stat.Value);
                        }
                    }

                    if (Items[i].Class.Stats != null && Items[i].Class.Stats.Count > 0)
                    {
                        foreach (var stat in Items[i].Class.Stats)
                        {
                            RegisterStat(stat.Value);
                        }
                    }

                    if (Items[i].Stats != null && Items[i].Stats.Count > 0)
                    {
                        foreach (var stat in Items[i].Stats)
                        {
                            RegisterStat(stat.Value);
                        }
                    }
                }
            }
        }

        private void RegisterStat(SO_Stat _Stat)
        {
            if (!UserStats.ContainsKey(_Stat.TargetUserStat))
            {
                UserStats.Add(_Stat.TargetUserStat, new Runtime_Stat(_Stat.GetStatValue(), _Stat.GetStatType()));
            }
            else if (_Stat.GetStatType().IsNumeric() && UserStats.ContainsKey(_Stat.TargetUserStat))
            {
                UserStats[_Stat.TargetUserStat].Value = (double.Parse(UserStats[_Stat.TargetUserStat].Value.ToString()) + double.Parse(_Stat.GetStatValue().ToString()));
            }
            else
            {
                UserStats[_Stat.TargetUserStat].Value = _Stat.GetStatValue();
            }
        }
    }
}