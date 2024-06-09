using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class UnitController : MonoBehaviour, IItemUser
{
    [SerializeField]
    [SerializedDictionary("Name", "Stat Value")]
    private SerializedDictionary<string, SO_Stat> m_unitStats = new SerializedDictionary<string, SO_Stat>();

    private SerializedDictionary<SO_Effect_Trigger, List<SO_Item_Effect>> m_effectRegistry = new SerializedDictionary<SO_Effect_Trigger, List<SO_Item_Effect>>();

    [SerializeField] private List<SO_Item> m_Items = new List<SO_Item>();
    private float timer = 1f;

    public List<SO_Item> Items { get => m_Items; set => m_Items = value; }

    SerializedDictionary<string, SO_Stat> IItemUser.UserStats { get => m_unitStats; set => m_unitStats = value; }

    SerializedDictionary<SO_Effect_Trigger, List<SO_Item_Effect>> IItemUser.EffectRegistry { get => m_effectRegistry; }

    private void Awake()
    {
        this.GetComponent<IItemUser>().OnInitialize();

        Debug.Log("Health: " + m_unitStats["Health"].GetValue());
        Debug.Log("Amor Type: " + m_unitStats["Armor"].GetValue());

        foreach (var item in m_effectRegistry)
        {
            Debug.Log(item.Key);
            foreach (var entry in item.Value)
            {
                Debug.Log(entry.EffectName);
            }
        }
    }

    private void Start()
    {
        ItemEventHandler.Instance.InvokeEvent<SO_Effect_Trigger_Interval>(this, this);
    }

    private void Update()
    {
        if (timer <= 0.0f)
        {
            ItemEventHandler.Instance.InvokeEvent<SO_Effect_Trigger_OnHit>(this, this);
            timer = 1.0f;
        }
        else 
        {
            timer -= Time.deltaTime;
        }
    }
}