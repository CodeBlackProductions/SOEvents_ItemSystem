using UnityEngine;

public abstract class SO_Item_Class : ScriptableObject
{
    [SerializeField] private string m_ClassName = "NewClass";
    [SerializeField] private SO_Class_Type m_Type;

    public string ClassName { get => m_ClassName; set => m_ClassName = value; }
    public SO_Class_Type Type { get => m_Type; set => m_Type = value; }
}