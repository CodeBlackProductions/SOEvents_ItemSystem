using UnityEditor;
using UnityEngine;

public class SO_EditorSettings : ScriptableObject, IItemModule
{
    [SerializeField] private string m_InstancesPath = "Assets/ItemSystem/Module_Instances";
    [SerializeField] private GUID m_GUID;

    [ItemToolkitAccess] public string InstancesPath { get => m_InstancesPath; set => m_InstancesPath = value; }




    public string ModuleName { get => "EditorSettings"; set => Debug.LogWarning("System modules should not be changed."); }
    public GUID ModuleGUID { get => m_GUID; set => Debug.LogWarning("System modules should not be changed."); }

    private void OnEnable()
    {
        m_GUID = GUID.Generate();
    }
}