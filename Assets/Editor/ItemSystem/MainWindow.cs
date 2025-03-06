using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MainWindow : EditorWindow
{
    private VisualElement m_Tab1Content;
    private VisualElement m_Tab2Content;
    private VisualElement m_Tab3Content;
    private List<Button> m_TabButtons = new List<Button>();

    [MenuItem("Window/Item System/Item System")]
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
        m_Tab1Content = new Tab1Controller();
        m_Tab2Content = new Tab2Controller();
        m_Tab3Content = new Tab3Controller();

        // Get placeholders for tab content
        var tab1ContentPlaceholder = root.Q<VisualElement>("tab1");
        var tab2ContentPlaceholder = root.Q<VisualElement>("tab2");
        var tab3ContentPlaceholder = root.Q<VisualElement>("tab3");

        tab1ContentPlaceholder.Add(m_Tab1Content);
        tab2ContentPlaceholder.Add(m_Tab2Content);
        tab3ContentPlaceholder.Add(m_Tab3Content);

        // Get tab buttons
        var tab1Button = root.Q<Button>("tab1Button");
        m_TabButtons.Add(tab1Button);
        var tab2Button = root.Q<Button>("tab2Button");
        m_TabButtons.Add(tab2Button);
        var tab3Button = root.Q<Button>("tab3Button");
        m_TabButtons.Add(tab3Button);

        // Initialize with the first tab
        ShowTab(tab1ContentPlaceholder, tab1Button);

        // Add click events to tab buttons
        tab1Button.clicked += () => ShowTab(tab1ContentPlaceholder, tab1Button);
        tab2Button.clicked += () => ShowTab(tab2ContentPlaceholder, tab2Button);
        tab3Button.clicked += () => ShowTab(tab3ContentPlaceholder, tab3Button);
    }

    private void ShowTab(VisualElement _TabContent, Button _TabButton)
    {
        // Hide all tabs
        foreach (var tab in rootVisualElement.Query<VisualElement>().ToList().Where(e => e.ClassListContains("tab-content")))
        {
            tab.RemoveFromClassList("active");
        }

        // Show selected tab
        _TabContent.AddToClassList("active");

        for (var i = 0; i < m_TabButtons.Count; i++) 
        {
            m_TabButtons[i].style.backgroundColor = Color.grey;
        }

        _TabButton.style.backgroundColor = Color.cyan;
    }
}
