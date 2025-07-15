using System;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Filter attribute for ItemToolkit to specify which items should be displayed in the toolkit.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ItemToolkitFilter : Attribute
    {
        public string[] InstanceNames { get; }
        public System.Type[] Types { get; }
        public bool Additive { get; }

        public ItemToolkitFilter(params string[] _InstanceNames)
        {
            InstanceNames = _InstanceNames;
            Additive = false;
        }

        public ItemToolkitFilter(bool _Additive, params string[] _InstanceNames)
        {
            InstanceNames = _InstanceNames;
            Additive = _Additive;
        }

        public ItemToolkitFilter(params System.Type[] _Types)
        {
            Types = _Types;
            Additive = false;
        }

        public ItemToolkitFilter(bool _Additive, params System.Type[] _Types)
        {
            Types = _Types;
            Additive = _Additive;
        }
    }
}