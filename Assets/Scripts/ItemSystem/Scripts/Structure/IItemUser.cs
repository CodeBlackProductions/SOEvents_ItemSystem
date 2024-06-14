using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using System.Diagnostics;

public interface IItemUser
{
    public List<SO_Item> Items { get; set; }
    public SerializedDictionary<string, SO_Stat> UserStats { get; set; }
    public SerializedDictionary<SO_Effect_Trigger, List<SO_Item_Effect>> EffectRegistry { get;}

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