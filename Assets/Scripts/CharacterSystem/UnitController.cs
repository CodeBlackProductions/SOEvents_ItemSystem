using AYellowpaper.SerializedCollections;
using UnityEngine;

public class UnitController : MonoBehaviour, IItemUser
{
    [SerializeField]
    [SerializedDictionary("Name", "Stat Value")]
    private SerializedDictionary<string, SO_Stat> unitStats = new SerializedDictionary<string, SO_Stat>();

    SerializedDictionary<string, SO_Stat> IItemUser.UserStats { get => unitStats; set => unitStats = value; }
}