using ItemSystem.Editor;
using ItemSystem.SubModules;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ItemSystem.MainModule
{
    /// <summary>
    /// Base effect trigger, used to handle <see cref="SO_Item_Effect"/> calls.
    /// Inherit from this when creating new effect triggers.
    /// </summary>
    public abstract class SO_Effect_Trigger : ScriptableObject, IItemModule
    {
        [SerializeField] private string m_TriggerName = "NewTrigger";
        [SerializeField] private GUID m_TriggerGUID;
        [SerializeField] private SO_Tag[] m_Tags;

        protected List<SO_Item_Effect> m_Listener = new List<SO_Item_Effect>();

        [ItemToolkitAccess] public string TriggerName { get => m_TriggerName; set => m_TriggerName = value; }
        [ItemToolkitAccess] public SO_Tag[] Tags { get => m_Tags; set => m_Tags = value; }

        public string ModuleName { get => m_TriggerName; set => m_TriggerName = value; }
        public GUID ModuleGUID { get => m_TriggerGUID; set => m_TriggerGUID = value; }

        public void OnEnable()
        {
            ItemEventHandler.Initialized += Initialize;
        }

        private void OnDisable()
        {
            ItemEventHandler.Instance?.RemoveEvent(this);
            m_Listener = new List<SO_Item_Effect>();
        }

        /// <summary>
        /// Initializes the trigger, after the <see cref="ItemEventHandler"/> is correctly initialized.
        /// </summary>
        private void Initialize()
        {
            ItemEventHandler.Instance?.RegisterEvent(this);
        }

        /// <summary>
        /// Registers an <see cref="SO_Item_Effect"/> as listener to this trigger event.
        /// </summary>
        /// <param name="_Effect"><see cref="SO_Item_Effect"/> to register as listener</param>
        public void RegisterEffect(SO_Item_Effect _Effect)
        {
            m_Listener.Add(_Effect);
        }

        /// <summary>
        /// Removes an <see cref="SO_Item_Effect"/> as listener from this trigger event.
        /// </summary>
        /// <param name="_Effect"><see cref="SO_Item_Effect"/> to remove as listener</param>
        public void RemoveEffect(SO_Item_Effect _Effect)
        {
            m_Listener.Remove(_Effect);
        }

        /// <summary>
        /// Checks custom condition, then calls custom functionality and registered listeners.
        /// </summary>
        /// <param name="_Source"><see cref="IItemUser"/> that called the trigger</param>
        /// <param name="_Target"><see cref="IItemUser"/> that should get targeted by the effects</param>
        /// <exception cref="System.Exception"></exception>
        public void Invoke(IItemUser _Source, IItemUser _Target)
        {
            if (m_Listener == null)
            {
                throw new System.Exception("Listeners not initialized in " + this.name);
            }

            if (m_Listener.Count > 0)
            {
                if (CheckCondition(_Source, _Target))
                {
                    if (CustomFunctionality(_Source, _Target))
                    {
                        for (int i = m_Listener.Count - 1; i >= 0; i--)
                        {
                            if (!CheckEffectRegistry(_Source, m_Listener[i]))
                            {
                                continue;
                            }

                            m_Listener[i]?.OnInvoke(_Source, _Target);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if _Source has effect registered and is thus allowed to call it.
        /// </summary>
        /// <param name="_Source"><see cref="IItemUser"/> to check for registered effect</param>
        /// <param name="_Listener"><see cref="SO_Item_Effect"/> to check for in registry</param>
        /// <returns></returns>
        protected bool CheckEffectRegistry(IItemUser _Source, SO_Item_Effect _Listener)
        {
            if (!_Source.EffectRegistry.ContainsKey(this))
            {
                return false;
            }
            return _Source.EffectRegistry[this].Contains(_Listener);
        }

        /// <summary>
        /// Override this to do custom checks before calling custom functionality and/or listeners.
        /// </summary>
        /// <param name="_Source"><see cref="IItemUser"/> that called the trigger</param>
        /// <param name="_Target"><see cref="IItemUser"/> that should get targeted by the effects</param>
        /// <returns>always returns true if not customized</returns>
        protected virtual bool CheckCondition(IItemUser _Source, IItemUser _Target)
        {
            return true;
        }

        /// <summary>
        /// Override this to add custom functionality to your trigger.
        /// </summary>
        /// <param name="_Source"><see cref="IItemUser"/> that called the trigger</param>
        /// <param name="_Target"><see cref="IItemUser"/> that should get targeted by the effects</param>
        /// <returns>Return true if you want to call registered listeners. Return false if you do so manually in here. Or do both if you're a madman.</returns>
        protected virtual bool CustomFunctionality(IItemUser source, IItemUser target)
        {
            return true;
        }
    }
}