using ItemSystem.SubModules;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
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

        public List<SO_Stat_Base> Stats { get; set; }

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
                }
            }

            SO_StatLoader statLoader = AssetDatabase.LoadAssetAtPath<SO_StatLoader>("Assets/ItemSystem/Sub Modules/StatModule/SO_StatLoader.asset");
            if (statLoader != null)
            {
                statLoader.LoadStats(this);
            }
        }
    }
}