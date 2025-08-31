using UnityEditor;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    public static class UI_Styles_Lib 
    {
        private static StyleSheet m_UI_Styles;
        public static StyleSheet GetUIStyles()
        {
            if (m_UI_Styles == null)
            {
                m_UI_Styles = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ItemSystem/Main Module/SystemStructure/Editor/Tabs and Windows/UI_Styles.uss");
            }
            return m_UI_Styles;
        }
    }
}