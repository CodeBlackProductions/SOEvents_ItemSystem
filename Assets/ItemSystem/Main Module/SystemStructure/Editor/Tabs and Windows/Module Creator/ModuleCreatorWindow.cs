using ItemSystem.MainModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Window for creating new item module instances.
    /// </summary>
    public class ModuleCreatorWindow : EditorWindow
    {
        private VisualElement m_Root;
        private DropdownField m_ModuleSelection;
        private DropdownField m_SubModuleSelection;
        private InspectorPanel m_InspectorPanel;
        private Button m_FinishButton;

        private static Dictionary<string, System.Type> m_ModuleTypeRegistry = new Dictionary<string, System.Type>();
        private static System.Type m_SelectedModuleType;

        private Action<bool> OnClose;

        public static void ShowWindow(Action<bool> _OnWindowClosedCallback)
        {
            m_SelectedModuleType = null;

            ModuleCreatorWindow window = CreateInstance<ModuleCreatorWindow>();
            window.titleContent = new GUIContent("Module Creator");
            window.minSize = new Vector2(700, 400);
            window.OnClose += _OnWindowClosedCallback;
            window.Show();
        }

        public static void ShowWindow(Action<bool> _OnWindowClosedCallback, System.Type _SelectedType)
        {
            m_SelectedModuleType = _SelectedType;

            ModuleCreatorWindow window = CreateInstance<ModuleCreatorWindow>();
            window.titleContent = new GUIContent("Module Creator");
            window.minSize = new Vector2(700, 400);
            window.OnClose += _OnWindowClosedCallback;
            window.Show();
        }

        private void CreateGUI()
        {
            m_Root = rootVisualElement;

            m_ModuleTypeRegistry.Clear();

            m_ModuleSelection = new DropdownField("Module type");

            IEnumerable<System.Type> moduleTypes = ItemEditor_AssetLoader.LoadAllBaseTypes();

            foreach (var type in moduleTypes)
            {
                if (m_ModuleTypeRegistry.TryAdd(GetModuleNameSubstring(type.Name), type))
                {
                    m_ModuleSelection?.choices?.Add(GetModuleNameSubstring(type.Name));
                }
            }

            m_ModuleSelection?.RegisterValueChangedCallback((evt) =>
            {
                System.Type type = m_ModuleTypeRegistry[evt.newValue];

                if (type == null)
                {
                    m_SubModuleSelection?.choices?.Clear();
                    m_SubModuleSelection?.choices?.Add("No submodules found!");
                    m_SubModuleSelection.value = "No submodules found!";
                    return;
                }

                MethodInfo method = typeof(ModuleCreatorWindow).GetMethod("SetupSubTypeSelection", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo generic = method.MakeGenericMethod(type);
                generic.Invoke(this, null);
            });

            m_SubModuleSelection = new DropdownField("Sub-Module type");
            m_SubModuleSelection?.choices?.Add("Select Main-Module first!");
            m_SubModuleSelection.value = "Select Main-Module first!";

            if (m_SelectedModuleType != null)
            {
                m_ModuleSelection.value = GetModuleNameSubstring(m_SelectedModuleType.Name);
                MethodInfo method = typeof(ModuleCreatorWindow).GetMethod("SetupSubTypeSelection", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo generic = method.MakeGenericMethod(m_SelectedModuleType);
                generic.Invoke(this, null);
            }
            else
            {
                m_ModuleSelection.value = "Select Module";
            }

            m_InspectorPanel = new InspectorPanel(0);

            m_FinishButton = new Button(() => this.Close());
            m_FinishButton?.Add(new Label("Finish"));

            m_Root?.Add(m_ModuleSelection);
            m_Root?.Add(m_SubModuleSelection);
            m_Root?.Add(m_InspectorPanel);
            m_Root?.Add(m_FinishButton);
        }

        private string GetModuleNameSubstring(string _ModuleName)
        {
            return _ModuleName.Substring(_ModuleName.LastIndexOf("_") + 1);
        }

        private void SetupSubTypeSelection<T>()
        {
            m_InspectorPanel?.Clear();
            m_SubModuleSelection?.choices?.Clear();

            List<System.Type> types = new List<System.Type>();
            System.Type panelType;

            types = ItemEditor_AssetLoader.LoadDerivedTypes(typeof(T)).ToList();
            panelType = typeof(T);

            m_SubModuleSelection.choices = types.Select(t => GetModuleNameSubstring(t.Name)).ToList();
            m_SubModuleSelection?.choices?.Insert(0, "Select Sub-Module");
            m_SubModuleSelection.value = "Select Sub-Module";

            m_SubModuleSelection?.RegisterValueChangedCallback((evt) => SetupInspectorPanel(types.Find((t) => GetModuleNameSubstring(t.Name) == evt.newValue), panelType));
        }

        private void SetupInspectorPanel(System.Type _SubType, System.Type _ModuleType)
        {
            if (_SubType == null) { return; }
            ScriptableObject temporarySOInstance = CreateTemporaryInstance(_SubType);
            m_InspectorPanel.Show(temporarySOInstance, null);

            m_FinishButton.clicked += () => FinishSetup(temporarySOInstance, _SubType);
        }

        private ScriptableObject CreateTemporaryInstance(System.Type _Type)
        {
            if (_Type != null && typeof(ScriptableObject).IsAssignableFrom(_Type))
            {
                ScriptableObject temporarySOInstance = ScriptableObject.CreateInstance(_Type);
                (temporarySOInstance as IItemModule).ModuleGUID = GUID.Generate();
                return temporarySOInstance;
            }
            else
            {
                Debug.LogError($"Could not create instance of type {_Type.Name}");
                return null;
            }
        }

        private void FinishSetup(ScriptableObject _TemporarySOInstance, System.Type _ModuleType)
        {
            if (_TemporarySOInstance != null)
            {
                ItemEditor_InstanceManager.CreateInstance(_TemporarySOInstance, _ModuleType);

                OnClose?.Invoke(true);
                Close();
            }
        }
    }
}