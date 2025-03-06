using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Tab3Controller : VisualElement
{
    private Button _openSecondaryWindowButton;

    public Tab3Controller()
    {
        // Load UXML and USS for Tab 3
        var tab1UXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/ItemSystem/Tab3Content.uxml");
        var tab1USS = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/ItemSystem/MainWindow.uss");

        var root = tab1UXML.CloneTree();
        root.styleSheets.Add(tab1USS);
        Add(root);

        // Get and configure components
        _openSecondaryWindowButton = root.Q<Button>("openSecondaryWindowButton");
        _openSecondaryWindowButton.clicked += OpenSecondaryWindow;
    }

    private void OpenSecondaryWindow()
    {
        // Create or show the SecondaryWindow
        var window = (SecondaryWindow)EditorWindow.GetWindow(typeof(SecondaryWindow));
        window.titleContent = new GUIContent("Secondary Window");
        window.Show();
    }
}
