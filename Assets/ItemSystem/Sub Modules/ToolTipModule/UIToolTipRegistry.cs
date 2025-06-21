using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ItemSystem.SubModules
{
    /// <summary>
    /// Singleton class to manage tooltips in the Item System.
    /// </summary>
    public class UIToolTipRegistry : MonoBehaviour
    {
        public static UIToolTipRegistry Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private Dictionary<string, string[]> m_ToolTipRegistry = new Dictionary<string, string[]>();

        private void Start()
        {
            List<SO_ToolTip> toolTips = LoadAssetsByType<SO_ToolTip>();
            foreach (var toolTip in toolTips)
            {
                if (!string.IsNullOrEmpty(toolTip.ToolTipID) && !string.IsNullOrEmpty(toolTip.ToolTipText))
                {
                    RegisterToolTip(toolTip.ToolTipID, new string[] { toolTip.HyperlinkText, toolTip.ToolTipText });
                }
            }
        }

        private List<T> LoadAssetsByType<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            return guids.Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid))).ToList();
        }

        public void RegisterToolTip(string _ID, string[] _Content)
        {
            m_ToolTipRegistry.Add(_ID, _Content);
        }

        public string RetrieveTooltip(string _ID)
        {
            m_ToolTipRegistry.TryGetValue(_ID, out string[] content);
            string text = content != null && content.Length > 1 ? content[1] : string.Empty;
            return text;
        }

        public string RetrieveTooltipHyperlinkText(string _ID)
        {
            m_ToolTipRegistry.TryGetValue(_ID, out string[] content);
            string text = content != null && content.Length > 1 ? content[0] : string.Empty;
            return text;
        }
    }
}