using ItemSystem.SubModules;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

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
            if (Items != null && Items.Count > 0)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    for (int e = 0; e < Items[i].Effects.Length; e++)
                    {
                        if (EffectRegistry.ContainsKey(Items[i].Effects[e].Trigger))
                        {
                            EffectRegistry[Items[i].Effects[e].Trigger].Add(Items[i].Effects[e]);
                        }
                        else
                        {
                            EffectRegistry.Add(Items[i].Effects[e].Trigger, new List<SO_Item_Effect>());
                            EffectRegistry[Items[i].Effects[e].Trigger].Add(Items[i].Effects[e]);
                        }
                    }

                    if (Items[i].Stats != null && Items[i].Stats.Count > 0)
                    {
                        foreach (var stat in Items[i].Stats)
                        {
                            if (!UserStats.ContainsKey(Stats[i].TargetUserStat))
                            {
                                UserStats.Add(Stats[i].TargetUserStat, new Runtime_Stat(Stats[i].GetStatValue(), Stats[i].GetStatType()));
                            }
                            else if (Stats[i].GetStatType().IsNumeric() && UserStats.ContainsKey(Stats[i].TargetUserStat))
                            {
                                UserStats[Stats[i].TargetUserStat].Value = (double.Parse(UserStats[Stats[i].TargetUserStat].Value.ToString()) + double.Parse(Stats[i].GetStatValue().ToString()));
                            }
                            else
                            {
                                Debug.Log($"{Stats[i].TargetUserStat} was already present.");
                            }
                        }
                    }

                    if (Items[i].Class.Stats != null && Items[i].Class.Stats.Count > 0)
                    {
                        foreach (var stat in Items[i].Class.Stats)
                        {
                            if (!UserStats.ContainsKey(Stats[i].TargetUserStat))
                            {
                                UserStats.Add(Stats[i].TargetUserStat, new Runtime_Stat(Stats[i].GetStatValue(), Stats[i].GetStatType()));
                            }
                            else if (Stats[i].GetStatType().IsNumeric() && UserStats.ContainsKey(Stats[i].TargetUserStat))
                            {
                                UserStats[Stats[i].TargetUserStat].Value = (double.Parse(UserStats[Stats[i].TargetUserStat].Value.ToString()) + double.Parse(Stats[i].GetStatValue().ToString()));
                            }
                            else
                            {
                                Debug.Log($"{Stats[i].TargetUserStat} was already present.");
                            }
                        }
                    }

                    if (Items[i].Class.Types[Items[i].TypeIndex].Stats != null && Items[i].Class.Types[Items[i].TypeIndex].Stats.Count > 0)
                    {
                        foreach (var stat in Items[i].Class.Types[Items[i].TypeIndex].Stats)
                        {
                            if (!UserStats.ContainsKey(Stats[i].TargetUserStat))
                            {
                                UserStats.Add(Stats[i].TargetUserStat, new Runtime_Stat(Stats[i].GetStatValue(), Stats[i].GetStatType()));
                            }
                            else if (Stats[i].GetStatType().IsNumeric() && UserStats.ContainsKey(Stats[i].TargetUserStat))
                            {
                                UserStats[Stats[i].TargetUserStat].Value = (double.Parse(UserStats[Stats[i].TargetUserStat].Value.ToString()) + double.Parse(Stats[i].GetStatValue().ToString()));
                            }
                            else
                            {
                                Debug.Log($"{Stats[i].TargetUserStat} was already present.");
                            }
                        }
                    }
                }
            }

            if (Stats != null && Stats.Count > 0)
            {
                for (int i = 0; i < Stats.Count; i++)
                {
                    if (!UserStats.ContainsKey(Stats[i].TargetUserStat))
                    {
                        UserStats.Add(Stats[i].TargetUserStat, new Runtime_Stat(Stats[i].GetStatValue(), Stats[i].GetStatType()));
                    }
                    else if (Stats[i].GetStatType().IsNumeric() && UserStats.ContainsKey(Stats[i].TargetUserStat)) 
                    {
                        UserStats[Stats[i].TargetUserStat].Value = (double.Parse(UserStats[Stats[i].TargetUserStat].Value.ToString()) + double.Parse(Stats[i].GetStatValue().ToString()));
                    }
                    else
                    {
                        Debug.Log($"{Stats[i].TargetUserStat} was already present.");
                    }
                }
            }
        }
    }
}