using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitController : MonoBehaviour, IItemUser
{
    [SerializeField]
    [SerializedDictionary("Name", "Stat Value")]
    private SerializedDictionary<string, SO_Stat> m_unitStats = new SerializedDictionary<string, SO_Stat>();

    private SerializedDictionary<SO_Effect_Trigger, List<SO_Item_Effect>> m_effectRegistry;

    private List<SO_Item> m_Items = new List<SO_Item>();

    public List<SO_Item> Items { get => m_Items; set => m_Items = value; }

    SerializedDictionary<string, SO_Stat> IItemUser.UserStats { get => m_unitStats; set => m_unitStats = value; }

    SerializedDictionary<SO_Effect_Trigger, List<SO_Item_Effect>> IItemUser.EffectRegistry { get => m_effectRegistry; }

    private void Awake()
    {
        this.GetComponent<IItemUser>().OnInitialize();
    }
}