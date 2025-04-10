using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ModuleCreatorWindow : EditorWindow
{
    private VisualElement m_Root;
    private DropdownField m_ModuleSelection;
    private DropdownField m_SubModuleSelection;
    private InspectorPanel m_InspectorPanel;
    private Button m_FinishButton;

    private Action<string> OnModuleChanged;
    private Action<bool> OnClose;

    public static void ShowWindow(Action<bool> _OnWindowClosedCallback)
    {
        ModuleCreatorWindow window = GetWindow<ModuleCreatorWindow>("Module Creator");
        window.minSize = new Vector2(700, 400);
        window.OnClose += _OnWindowClosedCallback;
    }

    private void CreateGUI()
    {
        m_Root = rootVisualElement;

        m_ModuleSelection = new DropdownField("Module type");
        m_ModuleSelection.choices = new List<string> { "Select Module", "Items", "Classes", "Types", "Effects", "Trigger", "ItemSlots", "ItemPools", "Stats" };
        m_ModuleSelection.RegisterValueChangedCallback((evt) => OnModuleChanged?.Invoke(evt.newValue));
        m_ModuleSelection.value = "Select Module";

        OnModuleChanged += SetupSubTypeSelection;

        m_SubModuleSelection = new DropdownField("Sub-Module type");
        m_SubModuleSelection.choices.Add("Select Main-Module first!");
        m_SubModuleSelection.value = "Select Main-Module first!";

        m_InspectorPanel = new InspectorPanel();

        m_FinishButton = new Button(() => this.Close());
        m_FinishButton.Add(new Label("Finish"));

        m_Root.Add(m_ModuleSelection);
        m_Root.Add(m_SubModuleSelection);
        m_Root.Add(m_InspectorPanel);
        m_Root.Add(m_FinishButton);
    }

    private void SetupSubTypeSelection(string _ModuleType)
    {
        m_InspectorPanel.Clear();
        m_SubModuleSelection.choices.Clear();

        if (_ModuleType == "Select Module")
        {
            return;
        }

        List<Type> types = new List<Type>();
        Type panelType;

        switch (_ModuleType)
        {
            case "Items":
                types = GetListOfSubTypes(typeof(SO_Item));
                panelType = typeof(SO_Item);
                break;

            case "Classes":
                types = GetListOfSubTypes(typeof(SO_Item_Class));
                panelType = typeof(SO_Item_Class);
                break;

            case "Types":
                types = GetListOfSubTypes(typeof(SO_Class_Type));
                panelType = typeof(SO_Class_Type);
                break;

            case "Effects":
                types = GetListOfSubTypes(typeof(SO_Item_Effect));
                panelType = typeof(SO_Item_Effect);
                break;

            case "Trigger":
                types = GetListOfSubTypes(typeof(SO_Effect_Trigger));
                panelType = typeof(SO_Effect_Trigger);
                break;

            case "ItemSlots":
                types = GetListOfSubTypes(typeof(SO_ItemSlot));
                panelType = typeof(SO_ItemSlot);
                break;

            case "ItemPools":
                throw new NotImplementedException();
            case "Stats":
                types = GetListOfSubTypes(typeof(SO_Stat));
                panelType = typeof(SO_Stat);
                break;

            default:
                Debug.LogError("Invalid ModuleType");
                return;
        }

        m_SubModuleSelection.choices = types.Select(t => t.Name).ToList();
        m_SubModuleSelection.choices.Insert(0, "Select Sub-Module");
        m_SubModuleSelection.value = "Select Sub-Module";

        m_SubModuleSelection.RegisterValueChangedCallback((evt) => SetupInspectorPanel(types.Find((t) => t.Name == evt.newValue), panelType));
    }

    private List<Type> GetListOfSubTypes(Type _BaseType)
    {
        return Assembly.GetAssembly(_BaseType)
            .GetTypes()
            .Where(t => t.IsSubclassOf(_BaseType) && !t.IsAbstract || t == _BaseType && !t.IsAbstract)
            .ToList();
    }

    private void SetupInspectorPanel(Type _SubType, Type _ModuleType)
    {
        if (_SubType == null) { return; }
        ScriptableObject temporarySOInstance = CreateTemporaryInstance(_SubType);
        m_InspectorPanel.Show(temporarySOInstance, null);

        m_FinishButton.clicked += () => FinishSetup(temporarySOInstance, _SubType);
    }

    private ScriptableObject CreateTemporaryInstance(Type _Type)
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

    private void FinishSetup(ScriptableObject _TemporarySOInstance, Type _ModuleType)
    {
        if (_TemporarySOInstance != null)
        {
            ItemEditor_InstanceManager.CreateInstance(_TemporarySOInstance, _ModuleType);

            OnClose?.Invoke(true);
            Close();
        }
    }
}