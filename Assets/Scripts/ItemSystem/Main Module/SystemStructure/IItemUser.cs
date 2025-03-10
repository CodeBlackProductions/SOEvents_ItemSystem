using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adds item system functionality to a custom unit/character script.
/// </summary>
public interface IItemUser
{
    public List<SO_Item> Items { get; set; }

    public List<SO_Stat> Stats { get; set; }

    public GameObject ImplementingUser { get; }
    public int Team { get; }

    public SerializedDictionary<string, Runtime_Stat> UserStats { get; }
    public SerializedDictionary<SO_Effect_Trigger, List<SO_Item_Effect>> EffectRegistry { get; }

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
            }
        }

        if (Stats != null && Stats.Count > 0)
        {
            for (int i = 0; i < Stats.Count; i++)
            {
                UserStats.Add(Stats[i].GetName(), new Runtime_Stat(Stats[i].GetValue(), Stats[i].GetStatType()));
            }
        }
    }
}