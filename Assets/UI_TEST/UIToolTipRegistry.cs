using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private Dictionary<string, string> m_ToolTipRegistry = new Dictionary<string, string>();

    public void RegisterToolTip(string _ID,string _Text)
    {
        m_ToolTipRegistry.Add(_ID, _Text);
    }

    public string RetrieveTooltip(string _ID) 
    {
        return m_ToolTipRegistry.TryGetValue(_ID, out string tooltipText) ? tooltipText : null;
    }
}
