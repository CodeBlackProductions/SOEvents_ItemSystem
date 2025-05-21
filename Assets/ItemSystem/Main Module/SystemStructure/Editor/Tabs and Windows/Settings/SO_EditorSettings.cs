using ItemSystem.MainModule;
using UnityEditor;
using UnityEngine;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Scriptable Object that stores the editor settings for the Item System.
    /// </summary>
    public class SO_EditorSettings : ScriptableObject, IItemModule
    {
        [SerializeField] private string m_InstancesPath = "Assets/ItemSystem/Module_Instances";
        [SerializeField] private string m_FilePath = Application.dataPath + "/ItemSystem/Module_JSON";
        [SerializeField] private GUID m_GUID;

        [ItemToolkitAccess] public string InstancesPath { get => m_InstancesPath; set => m_InstancesPath = value; }
        [ItemToolkitAccess] public string FilePath { get => m_FilePath; set => m_FilePath = value; }
        [ItemToolkitAccess] public SO_StatLoader.EBaseStatBehaviour BaseStatBehaviour { get => StatLoader.BaseStatBehaviour; set => StatLoader.BaseStatBehaviour = value; }
        [ItemToolkitAccess] public SO_StatLoader.ENonAddableStatBehaviour NonAddableBehaviour { get => StatLoader.OverrideBehaviour; set => StatLoader.OverrideBehaviour = value; }
        [ItemToolkitAccess] public SO_StatLoader.EStatLoadOrder LoadOrder { get => StatLoader.LoadOrder; set => StatLoader.LoadOrder = value; }

        public string ModuleName { get => "EditorSettings"; set => Debug.LogWarning("System modules should not be changed."); }
        public GUID ModuleGUID { get => m_GUID; set => Debug.LogWarning("System modules should not be changed."); }
        public SO_StatLoader StatLoader { get => ItemEditor_AssetLoader.LoadAssetByName<SO_StatLoader>("SO_StatLoader"); }

        private void OnEnable()
        {
            m_GUID = GUID.Generate();
        }
    }
}