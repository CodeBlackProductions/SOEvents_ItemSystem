using System;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Used to define which properties are accessible in the ItemToolkit.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ItemToolkitAccess : Attribute
    {
    }
}