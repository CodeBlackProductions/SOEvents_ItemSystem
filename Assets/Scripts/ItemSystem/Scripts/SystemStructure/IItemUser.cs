using AYellowpaper.SerializedCollections;

public interface IItemUser
{
    SerializedDictionary<string, SO_Stat> UserStats { get; set; }
}