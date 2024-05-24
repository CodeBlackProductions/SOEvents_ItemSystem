//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(SO_Item_Effect))]
//public class ItemEffectEditor : Editor
//{
//    SerializedProperty enumProperty;
//    SerializedProperty variableToHideProperty;

//    private void OnEnable()
//    {
//        enumProperty = serializedObject.FindProperty("m_EffectTarget");
//        variableToHideProperty = serializedObject.FindProperty("m_Range");
//    }

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        EditorGUILayout.PropertyField(enumProperty);

//        ETarget enumValue = (ETarget)enumProperty.enumValueIndex;

//        if (enumValue == ETarget.Range)
//        {
//            EditorGUILayout.PropertyField(variableToHideProperty);
//        }

//        serializedObject.ApplyModifiedProperties();
//    }
//}
