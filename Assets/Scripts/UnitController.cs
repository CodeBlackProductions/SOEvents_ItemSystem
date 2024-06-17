using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is just an example implementation of the item System.
/// Its not clean in any way. Please do not use this... create your own version.
/// </summary>
public class UnitController : MonoBehaviour, IItemUser
{
    [SerializeField]
    [SerializedDictionary("Name", "Stat Value")]
    private SerializedDictionary<string, SO_Stat> m_unitStats = new SerializedDictionary<string, SO_Stat>();

    private SerializedDictionary<SO_Effect_Trigger, List<SO_Item_Effect>> m_effectRegistry = new SerializedDictionary<SO_Effect_Trigger, List<SO_Item_Effect>>();

    [SerializeField] private SO_ItemSlot m_TestSlot;
    [SerializeField] private SO_Item m_TestItem;
    [SerializeField] private SO_Item m_WrongTestItem;
    [SerializeField] private GameObject m_TestTarget;

    private IItemUser m_TestTargetUser;

    [SerializeField] private List<SO_Item> m_Items = new List<SO_Item>();
    private float timer = 1f;

    public List<SO_Item> Items { get => m_Items; set => m_Items = value; }

    SerializedDictionary<string, SO_Stat> IItemUser.UserStats { get => m_unitStats; set => m_unitStats = value; }

    SerializedDictionary<SO_Effect_Trigger, List<SO_Item_Effect>> IItemUser.EffectRegistry { get => m_effectRegistry; }

    private void Awake()
    {
        if (m_TestSlot != null)
        {
            m_TestSlot.StoredItem = m_TestItem;
            m_TestSlot.StoredItem = m_WrongTestItem;

            Items.Add(m_TestSlot.StoredItem);
        }

        if (m_TestTarget != null)
        {
            m_TestTargetUser = m_TestTarget.GetComponent<IItemUser>();
        }

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
        if (m_TestTargetUser != null)
        {
            ItemEventHandler.Instance.InvokeEvent<SO_Effect_Trigger_OnHit>(this, m_TestTargetUser);
        }
    }

    private void Update()
    {

    }

}