using UnityEditor;

namespace ItemSystem.MainModule
{
    /// <summary>
    /// Required for all modules to implement. Abstract base classes already implement this.
    /// </summary>
    public interface IItemModule
    {
        public string ModuleName { get; set; }
        public GUID ModuleGUID { get; set; }
    }
}