using UnityEditor;

public interface IItemModule
{
    public string ModuleName { get; set; }
    public GUID ModuleGUID { get; set; }
}