using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MainWindow : EditorWindow
{
    private VisualElement _tab1Content;
    private VisualElement _tab2Content;

    [MenuItem("Window/Item System/Main Window")]
    public static void ShowExample()
    {
        var window = GetWindow<MainWindow>();
        window.titleContent = new GUIContent("Main Window");
    }

    public void OnEnable()
    {
        // Load and apply UXML and USS for Main Window
        var mainWindowUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/ItemSystem/MainWindow.uxml");
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/ItemSystem/MainWindow.uss");

        var root = mainWindowUXML.CloneTree();
        root.styleSheets.Add(styleSheet);
        rootVisualElement.Add(root);

        // Load Tab Controllers
        _tab1Content = new Tab1Controller();
        _tab2Content = new Tab2Controller();

        // Get placeholders for tab content
        var tab1ContentPlaceholder = root.Q<VisualElement>("tab1");
        var tab2ContentPlaceholder = root.Q<VisualElement>("tab2");

        tab1ContentPlaceholder.Add(_tab1Content);
        tab2ContentPlaceholder.Add(_tab2Content);

        // Get tab buttons
        var tab1Button = root.Q<Button>("tab1Button");
        var tab2Button = root.Q<Button>("tab2Button");

        // Initialize with the first tab
        ShowTab(tab1ContentPlaceholder);

        // Add click events to tab buttons
        tab1Button.clicked += () => ShowTab(tab1ContentPlaceholder);
        tab2Button.clicked += () => ShowTab(tab2ContentPlaceholder);
    }

    private void ShowTab(VisualElement tabContent)
    {
        // Hide all tabs
        foreach (var tab in rootVisualElement.Query<VisualElement>().ToList().Where(e => e.ClassListContains("tab-content")))
        {
            tab.RemoveFromClassList("active");
        }

        // Show selected tab
        tabContent.AddToClassList("active");
    }
}
