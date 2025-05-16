using System.Collections.Generic;
using UnityEngine;

namespace ItemSystem.MainModule
{
    /// <summary>
    /// Automatically handles events from the item system. Call this to invoke item functionality.
    /// </summary>
    public class ItemEventHandler : MonoBehaviour
    {
        private static ItemEventHandler m_instance;

        public static ItemEventHandler Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = FindObjectOfType<ItemEventHandler>();

                    if (m_instance == null)
                    {
                        GameObject singletonObject = new GameObject("ItemEventHandler");
                        m_instance = singletonObject.AddComponent<ItemEventHandler>();
                    }
                }

                return m_instance;
            }
        }

        public static event System.Action Initialized;

        private Dictionary<System.Type, List<ScriptableObject>> events = new Dictionary<System.Type, List<ScriptableObject>>();

        /// <summary>
        /// Registers an <see cref="SO_Effect_Trigger"/> as event.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="SO_Effect_Trigger"/> to register</typeparam>
        /// <param name="_eventSO"><see cref="SO_Effect_Trigger"/> to register</param>
        public void RegisterEvent<T>(T _eventSO) where T : ScriptableObject
        {
            var type = _eventSO.GetType();
            if (!events.ContainsKey(type))
            {
                events.Add(type, new List<ScriptableObject>() { _eventSO });
            }
            else if (!events[type].Contains(_eventSO))
            {
                events[type].Add(_eventSO);
            }
        }

        /// <summary>
        /// Removes an event from registry based on its related <see cref="SO_Effect_Trigger"/>.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="SO_Effect_Trigger"/> to remove</typeparam>
        /// <param name="_eventSO"><see cref="SO_Effect_Trigger"/> to remove</param>
        public void RemoveEvent<T>(T _eventSO) where T : ScriptableObject
        {
            var type = _eventSO.GetType();
            if (events.ContainsKey(type) && events[type].Contains(_eventSO))
            {
                events[type].Remove(_eventSO);
                if (events[type].Count <= 0)
                {
                    events.Remove(type);
                }
            }
        }

        /// <summary>
        /// Invokes a registered <see cref="SO_Effect_Trigger"/> event.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="SO_Effect_Trigger"/> event to invoke</typeparam>
        /// <param name="_Source"><see cref="IItemUser"/> that invokes the effect</param>
        /// <param name="_Target"><see cref="IItemUser"/> that gets targeted by the effect</param>
        public void InvokeEvent<T>(IItemUser _Source, IItemUser _Target) where T : SO_Effect_Trigger
        {
            List<ScriptableObject> eventSOs = GetEvents<T>();

            foreach (var SO in eventSOs)
            {
                (SO as T).Invoke(_Source, _Target);
            }
        }

        /// <summary>
        /// Fetches an <see cref="SO_Effect_Trigger"/> evént from the registry.
        /// </summary>
        /// <typeparam name="T">Type of <see cref="SO_Effect_Trigger"/> to fetch</typeparam>
        /// <returns><see cref="SO_Effect_Trigger"/> event</returns>
        private List<ScriptableObject> GetEvents<T>() where T : ScriptableObject
        {
            var type = typeof(T);
            if (events.ContainsKey(type))
            {
                return events[type];
            }
            return null;
        }

        /// <summary>
        /// Calls initialize function for scriptable objects that need to be initialized after the event handler.
        /// </summary>
        private void Awake()
        {
            Initialized?.Invoke();
        }
    }
}