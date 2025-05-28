using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFactory : MonoBehaviour
{
    public static UIFactory Instance { get; private set; }

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

    private void Start()
    {
        UIToolTipRegistry.Instance.RegisterToolTip("Test", "This is a test hyperlink for {Damage} values.");
        UIToolTipRegistry.Instance.RegisterToolTip("Damage", "Damage ist besser als {Schaden}.");
        UIToolTipRegistry.Instance.RegisterToolTip("Schaden", "wow, it be workin.");

        GameObject ui = CreateNewUI("newUI", UIToolTipRegistry.Instance.RetrieveTooltip("Test"), Color.white, Color.blue, 36);
        ui.transform.SetParent(transform, false);
    }

    public GameObject CreateNewUI(string _UIName, string _Text, Color _TextColor, Color _HyperlinkColor, int _FontSize)
    {
        GameObject newUI = new GameObject(_UIName);
        Canvas canvas = newUI.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        newUI.AddComponent<GraphicRaycaster>();

        GameObject backgroundObj = new GameObject(_UIName + "_Background");
        Image bg = backgroundObj.AddComponent<Image>();
        bg.color = Color.gray;
        bg.rectTransform.anchorMin = new Vector2(0f, 0f);
        bg.rectTransform.anchorMax = new Vector2(1f, 1f);
        bg.raycastTarget = false;
        bg.transform.SetParent(canvas.transform, false);
        bg.transform.SetAsFirstSibling();

        GameObject textObj = new GameObject(_UIName + "_Text");
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = CreateHyperlinks(_Text, _HyperlinkColor);
        text.fontSize = _FontSize;
        text.color = _TextColor;
        text.rectTransform.anchorMin = new Vector2(0.25f, 0.1f);
        text.rectTransform.anchorMax = new Vector2(0.75f, 0.9f);
        text.richText = true;
        text.raycastTarget = true;
        text.transform.SetParent(canvas.transform, false);
        text.transform.SetAsLastSibling();

        return newUI;
    }

    private string CreateHyperlinks(string _Text, Color _Color)
    {
        string text = _Text;
        List<string> substrings = GetSubstringsInBraces(text);

        foreach (string subTTID in substrings)
        {
            string subTTContent = UIToolTipRegistry.Instance.RetrieveTooltip(subTTID);
            text = text.Replace("{" + subTTID + "}", $"<link=\"{subTTID}\"><color=#{ColorUtility.ToHtmlStringRGBA(_Color)}>{subTTID}</color></link>");
        }

        return text;
    }

    private List<string> GetSubstringsInBraces(string input)
    {
        var matches = Regex.Matches(input, @"\{([^}]*)\}");
        var results = new List<string>();
        foreach (Match match in matches)
        {
            results.Add(match.Groups[1].Value);
        }
        return results;
    }
}