using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SecondaryWindow : EditorWindow
{
    public static void ShowExample()
    {
        var window = GetWindow<SecondaryWindow>();
        window.titleContent = new GUIContent("Secondary Window");
        window.Show();
    }

    public void OnEnable()
    {
        // Load and apply UXML and USS for SecondaryWindow
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/ItemSystem/SecondaryWindow.uxml");
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/ItemSystem/SecondaryWindow.uss");

        var root = visualTree.CloneTree();
        root.styleSheets.Add(styleSheet);
        rootVisualElement.Add(root);

        // Get the button and add click event
        var closeButton = root.Q<Button>("closeButton");
        closeButton.clicked += () => { Close(); };
    }
}
