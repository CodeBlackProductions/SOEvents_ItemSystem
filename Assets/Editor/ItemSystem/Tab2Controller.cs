using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Tab2Controller : VisualElement
{
    public Tab2Controller()
    {
        // Load UXML and USS for Tab 2
        var tab2UXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/ItemSystem/Tab2Content.uxml");
        var tab2USS = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/ItemSystem/MainWindow.uss");

        var root = tab2UXML.CloneTree();
        root.styleSheets.Add(tab2USS);
        Add(root);
    }
}
