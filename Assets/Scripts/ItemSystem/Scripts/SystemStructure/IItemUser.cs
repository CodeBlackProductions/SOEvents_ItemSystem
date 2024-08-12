using AYellowpaper.SerializedCollections;
using System.Collections.Generic;

/// <summary>
/// Adds item system functionality to a custom unit/character script.
/// </summary>
public interface IItemUser
{
    public List<SO_Item> Items { get; set; }
    public SerializedDictionary<string, SO_Stat> UserStats { get; set; }
    public SerializedDictionary<SO_Effect_Trigger, List<SO_Item_Effect>> EffectRegistry { get; }

    /// <summary>
    /// Initializes the item effects into the registry. Needed for checking if an effect has to be triggered later on.
    /// </summary>
    public void OnInitialize()
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
}