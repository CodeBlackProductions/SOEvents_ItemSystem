using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

[CustomEditor(typeof(ScriptableObject), true)]
public class SO_Editor : Editor
{
    private HashSet<ScriptableObject> displayedObjects = new HashSet<ScriptableObject>();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ScriptableObject targetSO = (ScriptableObject)target;

        // Display the contents of the selected ScriptableObject
        DisplayScriptableObjectContents(targetSO);
    }

    private void DisplayScriptableObjectContents(ScriptableObject scriptableObject)
    {
        EditorGUILayout.Space();

        // Get or generate a color for the ScriptableObject
        Color sectionHeaderColor = Color.grey;

        // Display the contents of each ScriptableObject field
        EditorGUILayout.LabelField(scriptableObject.name + " Contents", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        // Get the fields of the ScriptableObject
        var fields = scriptableObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Iterate through each field
        foreach (var field in fields)
        {
            // Check if the field type is a ScriptableObject
            if (typeof(ScriptableObject).IsAssignableFrom(field.FieldType))
            {
                DisplaySingleScriptableObjectContents(field.GetValue(scriptableObject) as ScriptableObject, sectionHeaderColor);
            }
            else if (field.FieldType.IsArray && typeof(ScriptableObject).IsAssignableFrom(field.FieldType.GetElementType()))
            {
                DisplayArrayOfScriptableObjects(field.GetValue(scriptableObject) as ScriptableObject[], sectionHeaderColor);
            }
        }

        EditorGUI.indentLevel--;
    }

    private void DisplaySingleScriptableObjectContents(ScriptableObject scriptableObject, Color sectionHeaderColor)
    {
        if (scriptableObject == null)
            return;

        EditorGUILayout.Space();
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), sectionHeaderColor);
        EditorGUILayout.LabelField(scriptableObject.name + " Contents", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;

        // Display the properties of the referenced ScriptableObject
        SerializedObject nestedObjectSerialized = new SerializedObject(scriptableObject);
        SerializedProperty property = nestedObjectSerialized.GetIterator();
        bool enterChildren = true;
        while (property.NextVisible(enterChildren))
        {
            // Skip script property
            if (property.name == "m_Script")
                continue;

            EditorGUILayout.PropertyField(property, true);
            enterChildren = false;
        }

        EditorGUI.indentLevel--;

        // Apply modified properties of the nested ScriptableObject
        nestedObjectSerialized.ApplyModifiedProperties();

        // Add the displayed ScriptableObject to the HashSet
        displayedObjects.Add(scriptableObject);
    }

    private void DisplayArrayOfScriptableObjects(ScriptableObject[] array, Color sectionHeaderColor)
    {
        if (array == null || array.Length == 0)
            return;

        EditorGUILayout.Space();
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), sectionHeaderColor);
        EditorGUILayout.LabelField("Array of Scriptable Objects", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;

        // Display each element of the array
        for (int i = 0; i < array.Length; i++)
        {
            DisplaySingleScriptableObjectContents(array[i], sectionHeaderColor);
        }

        EditorGUI.indentLevel--;
    }
}
