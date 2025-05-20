using ItemSystem.Editor;
using ItemSystem.MainModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEffectModule
{
    [ItemToolkitAccess] public SO_Item_Effect[] Effects { get; set; }
}
