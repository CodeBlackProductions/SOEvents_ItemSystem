using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
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

        GameObject ui = CreateNewUIWindow("newUI", UIToolTipRegistry.Instance.RetrieveTooltip("Test"), Color.white, Color.blue, 36);
        ui.transform.SetParent(transform, false);
    }

    public GameObject CreateNewUIWindow(string _UIName, string _Text, Color _TextColor, Color _HyperlinkColor, int _FontSize)
    {
        GameObject newUI = new GameObject(_UIName);
        Canvas canvas = newUI.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        newUI.AddComponent<GraphicRaycaster>();

        GameObject backgroundObj = new GameObject(_UIName + "_Background");
        Image bg = backgroundObj.AddComponent<Image>();
        bg.transform.SetParent(canvas.transform, false);
        bg.color = Color.gray;
        bg.rectTransform.anchorMin = new Vector2(0f, 0f);
        bg.rectTransform.anchorMax = new Vector2(1f, 1f);
        bg.raycastTarget = false;
        bg.transform.SetAsFirstSibling();


        GameObject textObj = CreateNewTextElement(_UIName,_Text,_TextColor,_HyperlinkColor,_FontSize);
        TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
        text.transform.SetParent(canvas.transform, false);
        text.transform.SetAsLastSibling();

        CreateNewWindowBorder(_UIName, canvas, null, Color.black);

        CreateNewCloseWindowButton(_UIName,canvas,null,Color.red);

        return newUI;
    }

    public GameObject CreateNewTextElement(string _UIName, string _Text, Color _TextColor, Color _HyperlinkColor, int _FontSize)
    {
        GameObject textObj = new GameObject(_UIName + "_Text");
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = CreateHyperlinks(_Text, _HyperlinkColor);
        text.fontSize = _FontSize;
        text.color = _TextColor;
        text.rectTransform.anchorMin = new Vector2(0.25f, 0.1f);
        text.rectTransform.anchorMax = new Vector2(0.75f, 0.9f);
        text.richText = true;
        text.raycastTarget = true;

        return textObj;
    }

    private void CreateNewWindowBorder(string _UIName, Canvas _Parent, Sprite _BorderImage, Color _BorderColor)
    {
        GameObject borderObj = new GameObject(_UIName + "_Border");
        Image topBorder = borderObj.AddComponent<Image>();
        if (_BorderImage != null)
        {
            topBorder.sprite = _BorderImage;
        }
        topBorder.transform.SetParent(_Parent.transform, false);
        topBorder.color = _BorderColor;
        topBorder.rectTransform.anchorMin = new Vector2(0f, 0.96f);
        topBorder.rectTransform.anchorMax = new Vector2(1f, 1f);
        topBorder.raycastTarget = true;
        topBorder.transform.SetAsLastSibling();
    }

    private void CreateNewCloseWindowButton(string _UIName, Canvas _Parent, Sprite _ButtonImage, Color _ButtonColor) 
    {
        GameObject closeWindowObject = new GameObject(_UIName + "_CloseWindow");
        Button btn_CloseWindow = closeWindowObject.AddComponent<Button>();
        btn_CloseWindow.transform.SetParent(_Parent.transform, false);
        btn_CloseWindow.image = btn_CloseWindow.AddComponent<Image>();
        if (_ButtonImage != null)
        {
            btn_CloseWindow.image.sprite = _ButtonImage;
        }
        btn_CloseWindow.image.color = _ButtonColor;
        btn_CloseWindow.image.rectTransform.anchorMin = new Vector2(0.98f, 0.98f);
        btn_CloseWindow.image.rectTransform.anchorMax = new Vector2(1f, 1f);
        btn_CloseWindow.transform.SetAsLastSibling();
        btn_CloseWindow.onClick.AddListener(() =>
        {
            Destroy(btn_CloseWindow.transform.parent.gameObject);
        });
    }

    private string CreateHyperlinks(string _Text, Color _Color)
    {
        string text = _Text;
        List<string> substrings = GetSubstringsInBraces(text);

        foreach (string subTTID in substrings)
        {
            string subTTContent = UIToolTipRegistry.Instance.RetrieveTooltip(subTTID);
            text = text.Replace("{" + subTTID + "}", $"<link=\"{subTTID}\"><color=#{UnityEngine.ColorUtility.ToHtmlStringRGBA(_Color)}>{subTTID}</color></link>");
        }

        return text;
    }

    private List<string> GetSubstringsInBraces(string _Input)
    {
        var matches = Regex.Matches(_Input, @"\{([^}]*)\}");
        var results = new List<string>();
        foreach (Match match in matches)
        {
            results.Add(match.Groups[1].Value);
        }
        return results;
    }
}