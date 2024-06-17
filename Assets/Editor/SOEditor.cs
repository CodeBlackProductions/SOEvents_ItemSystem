using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScriptableObject), true)]
public class SOEditor : Editor
{
    private HashSet<ScriptableObject> displayedObjects = new HashSet<ScriptableObject>();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ScriptableObject targetSO = (ScriptableObject)target;

        DisplayScriptableObjectContents(targetSO);
    }

    private void DisplayScriptableObjectContents(ScriptableObject scriptableObject)
    {
        EditorGUILayout.Space();

        Color sectionHeaderColor = Color.grey;

        EditorGUILayout.LabelField(scriptableObject.name + " Contents", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        var fields = scriptableObject.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

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

            if (scriptableObject.GetType() == typeof(SO_Item) && field.Name == "m_Class")
            {
                DisplayTypeDropdown(scriptableObject as SO_Item, sectionHeaderColor);
            }
        }

        EditorGUI.indentLevel--;
    }

    private void DisplayTypeDropdown(SO_Item item, Color sectionHeaderColor)
    {
        if (item.Class.Types != null && item.Class.Types.Length > 0)
        {
            string[] typeNames = new string[item.Class.Types.Length];
            for (int i = 0; i < item.Class.Types.Length; i++)
            {
                typeNames[i] = item.Class.Types[i].name;
            }

            int selectedIndex = EditorGUILayout.Popup("Select Type", item.TypeIndex, typeNames);
            if (selectedIndex != item.TypeIndex)
            {
                Undo.RecordObject(item, "Change Item Type");
                item.TypeIndex = selectedIndex;
                EditorUtility.SetDirty(item);
            }
        }
        else
        {
            EditorGUILayout.LabelField("No Types Defined", EditorStyles.miniLabel);
        }
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