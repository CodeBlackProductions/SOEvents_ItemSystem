using System;
using System.Collections.Generic;
using UnityEngine;
using ItemSystem.MainModule;
using ItemSystem.SubModules;

/// <summary>
/// This is just an example implementation of the item System.
/// Its not clean in any way. Please do not use this... create your own version.
/// </summary>
public class UnitController : MonoBehaviour, IItemUser
{
    private Dictionary<SO_Effect_Trigger, List<SO_Item_Effect>> m_effectRegistry = new Dictionary<SO_Effect_Trigger, List<SO_Item_Effect>>();
    private Dictionary<string, Runtime_Stat> m_UserStats = new Dictionary<string, Runtime_Stat>();

    [SerializeField] private List<SO_Stat> m_unitStats;
    [SerializeField] private SO_ItemSlot m_TestSlot;
    [SerializeField] private SO_Item m_TestItem;
    [SerializeField] private SO_Item m_WrongTestItem;
    [SerializeField] private GameObject m_TestTarget;
    [SerializeField] private int m_Team = 0;

    private IItemUser m_TestTargetUser;

    private List<SO_Item> m_Items = new List<SO_Item>();

    public List<SO_Item> Items { get => m_Items; set => m_Items = value; }
    public List<SO_Stat> Stats { get => m_unitStats; set => m_unitStats = value; }

    public Dictionary<string, Runtime_Stat> UserStats { get => m_UserStats; }

    public GameObject ImplementingUser => this.gameObject;

    public int Team => m_Team;

    Dictionary<SO_Effect_Trigger, List<SO_Item_Effect>> IItemUser.EffectRegistry { get => m_effectRegistry; }

    private ItemEventHandler m_ItemEventHandler;

    private void Awake()
    {
        m_ItemEventHandler = ItemEventHandler.Instance;

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

        Type healthType = UserStats["Health"].Type;
        var health = UserStats["Health"].Value;

        Type armorType = UserStats["Armor"].Type;
        var armor = UserStats["Armor"].Value;

        Type typeOfArmorType = UserStats["Armor Type"].Type;
        var typeOfArmor = UserStats["Armor Type"].Value;

        Debug.Log("Health: " + health + " of type: " + healthType.Name);
        Debug.Log("Armor: " + armor + " of type: " + armorType.Name);
        Debug.Log("Armor Type: " + typeOfArmor + " of type: " + typeOfArmorType.Name);

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