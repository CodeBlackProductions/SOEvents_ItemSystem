using ItemSystem.MainModule;
using ItemSystem.SubModules;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is just an example implementation of the item System.
/// Its not clean in any way. Please do not use this... create your own version.
/// </summary>
public class UnitController : MonoBehaviour, IItemUser
{
    private Dictionary<SO_Effect_Trigger, List<SO_Item_Effect>> m_effectRegistry = new Dictionary<SO_Effect_Trigger, List<SO_Item_Effect>>();
    private Dictionary<string, Runtime_Stat> m_UserStats = new Dictionary<string, Runtime_Stat>();

    [SerializeField] private List<SO_Stat_Base> m_unitStats;
    [SerializeField] private SO_ItemSlot m_TestSlot;
    [SerializeField] private SO_Item m_TestItem1;
    [SerializeField] private SO_Item m_TestItem2;
    [SerializeField] private GameObject m_TestTarget;
    [SerializeField] private int m_Team = 0;

    private IItemUser m_TestTargetUser;

    private List<SO_Item> m_Items = new List<SO_Item>();

    public List<SO_Item> Items { get => m_Items; set => m_Items = value; }
    public List<SO_Stat_Base> Stats { get => m_unitStats; set => m_unitStats = value; }

    public Dictionary<string, Runtime_Stat> UserStats { get => m_UserStats; }

    public GameObject ImplementingUser => this.gameObject;

    public int Team => m_Team;

    Dictionary<SO_Effect_Trigger, List<SO_Item_Effect>> IItemUser.EffectRegistry { get => m_effectRegistry; }

    private ItemEventHandler m_ItemEventHandler;

    private void Start()
    {
        m_ItemEventHandler = ItemEventHandler.Instance;

        if (m_TestSlot != null)
        {
            m_TestSlot.StoredItem = m_TestItem1;
            m_TestSlot.StoredItem = m_TestItem2;

            Items.Add(m_TestSlot.StoredItem);
        }

        if (m_TestTarget != null)
        {
            m_TestTargetUser = m_TestTarget.GetComponent<IItemUser>();
        }

        this.GetComponent<IItemUser>().OnInitialize();

        foreach (var stat in UserStats)
        {
            Debug.Log($"Stat: {stat.Key}: {stat.Value.Value}");
        }

        foreach (var item in m_effectRegistry)
        {
            Debug.Log("Added:" + item.Key.TriggerName);
            foreach (var entry in item.Value)
            {
                Debug.Log("Added:" + entry.EffectName);
            }
        }

        foreach (SO_Item item in Items)
        {
            UIController.Instance.LoadTooltip(item);
        }

        ItemEventHandler.Instance.InvokeEvent<SO_Effect_Trigger_Interval>(this, this);
        if (m_TestTargetUser != null)
        {
            ItemEventHandler.Instance.InvokeEvent<SO_Effect_Trigger_OnHit>(this, m_TestTargetUser);
        }
    }
}