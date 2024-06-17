using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Shows content of all nested Scriptable objects within the selected Scriptable object and lets you edit them.
/// DO NOT TOUCH!
/// Unless you know what you are doing...
/// </summary>
[CustomEditor(typeof(ScriptableObject), true)]
public class SOEditor : Editor
{
    private HashSet<ScriptableObject> displayedObjects = new HashSet<ScriptableObject>();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ScriptableObject targetSO = (ScriptableObject)target;

        if (targetSO != null && targetSO.GetType() == typeof(SO_ItemSlot))
        {
            return;
        }

        DisplayScriptableObjectContents(targetSO);
    }

    private void DisplayScriptableObjectContents(ScriptableObject scriptableObject)
    {
        EditorGUILayout.Space();

        Color sectionHeaderColor = Color.grey;

        EditorGUILayout.LabelField(scriptableObject.name + " Contents", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        var fields = scriptableObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
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

        SerializedObject nestedObjectSerialized = new SerializedObject(scriptableObject);
        SerializedProperty property = nestedObjectSerialized.GetIterator();
        bool enterChildren = true;
        while (property.NextVisible(enterChildren))
        {
            if (property.name == "m_Script")
                continue;

            EditorGUILayout.PropertyField(property, true);

            // Handle nested properties recursively
            if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null)
            {
                DisplaySingleScriptableObjectContents(property.objectReferenceValue as ScriptableObject, sectionHeaderColor);
            }

            enterChildren = false;
        }

        EditorGUI.indentLevel--;

        nestedObjectSerialized.ApplyModifiedProperties();
    }

    private void DisplayArrayOfScriptableObjects(ScriptableObject[] array, Color sectionHeaderColor)
    {
        if (array == null || array.Length == 0)
            return;

        EditorGUILayout.Space();
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), sectionHeaderColor);
        EditorGUILayout.LabelField("Array of Scriptable Objects", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;

        for (int i = 0; i < array.Length; i++)
        {
            DisplaySingleScriptableObjectContents(array[i], sectionHeaderColor);
        }

        EditorGUI.indentLevel--;
    }
}