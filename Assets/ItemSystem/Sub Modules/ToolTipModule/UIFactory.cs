using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ItemSystem.SubModules
{
    /// <summary>
    /// Static class to create and manage UI windows with tooltips and hyperlinks.
    /// </summary>
    public static class UIFactory
    {
        public static GameObject CreateNewUIWindow(string _UIName, Vector3 _MousePos, Vector2 _Pivot, Vector2 _ScreenPercent, string _HeaderText, string _Text, Color _TextColor, Color _HyperlinkColor, int _FontSize)
        {
            Vector2 _ScreenPos = new Vector2(_MousePos.x / Screen.width, _MousePos.y / Screen.height);

            GameObject newUI = CreateNewUIWindow(_UIName, _ScreenPos, _Pivot, _ScreenPercent, _HeaderText, _Text, _TextColor, _HyperlinkColor, _FontSize);

            return newUI;
        }

        public static GameObject CreateNewUIWindow(string _UIName, Vector2 _ScreenPos, Vector2 _Pivot, Vector2 _ScreenPercent, string _HeaderText, string _Text, Color _TextColor, Color _HyperlinkColor, int _FontSize)
        {
            GameObject newUI = new GameObject(_UIName);
            Canvas canvas = newUI.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            newUI.AddComponent<GraphicRaycaster>();

            GameObject uiContainer = new GameObject(_UIName + "_Container");
            uiContainer.transform.SetParent(canvas.transform, false);
            RectTransform rt = uiContainer.AddComponent<RectTransform>();
            ResetRectTransform(uiContainer);
            SetSizeByScreenPercentage(rt, _ScreenPercent, _ScreenPos, _Pivot);
            rt.anchoredPosition = Vector2.zero;

            CreateUIElement<Image>(_UIName, "Background_Border", uiContainer, null, Color.black, new Vector2(0f, 0f), new Vector2(1f, 1f), true, false);
            CreateUIElement<Image>(_UIName, "Background", uiContainer, null, Color.gray, new Vector2(0.01f, 0.01f), new Vector2(0.99f, 0.99f), true, false);

            GameObject textObj = CreateNewTextElement(_UIName, _Text, _TextColor, _HyperlinkColor, _FontSize, new Vector2(0.025f, 0.1f), new Vector2(0.95f, 0.85f), true);
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            text.transform.SetParent(uiContainer.transform, false);
            ResetRectTransform(textObj);
            text.transform.SetAsLastSibling();

            Button btn_TopBorder = CreateUIElement<Button>(_UIName, "TopBorder", uiContainer, null, Color.black, new Vector2(0f, 0.9f), new Vector2(1f, 1f), true, true).GetComponent<Button>();
            EventTrigger trigger = btn_TopBorder.gameObject.AddComponent<EventTrigger>();
            Vector2 dragOffset = Vector2.zero;
            EventTrigger.Entry beginDrag = new EventTrigger.Entry();
            beginDrag.eventID = EventTriggerType.BeginDrag;
            beginDrag.callback.AddListener((data) =>
            {
                Vector2 screenPos = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
                dragOffset = screenPos - rt.anchorMin;
            });
            EventTrigger.Entry drag = new EventTrigger.Entry();
            drag.eventID = EventTriggerType.Drag;
            drag.callback.AddListener((data) =>
            {
                Vector2 screenPos = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
                Vector2 newPos = screenPos - dragOffset;
                rt.anchorMin = rt.anchorMax = newPos;
            });
            trigger.triggers.Add(beginDrag);
            trigger.triggers.Add(drag);

            GameObject headerObj = CreateNewTextElement(_UIName + "_Header_", _HeaderText, Color.white, _HyperlinkColor, 18, new Vector2(0.02f,0.92f), new Vector2(0.98f, 0.98f), false);
            TextMeshProUGUI headerText = headerObj.GetComponent<TextMeshProUGUI>();
            headerText.transform.SetParent(uiContainer.transform, false);
            ResetRectTransform(headerObj);
            headerText.transform.SetAsLastSibling();

            Button btn_CloseWindow = CreateUIElement<Button>(_UIName, "CloseWindow", uiContainer, null, Color.red, new Vector2(0.94f, 0.92f), new Vector2(0.98f, 0.98f), true, true).GetComponent<Button>();
            btn_CloseWindow.onClick.AddListener(() =>
            {
                GameObject.Destroy(btn_CloseWindow.transform.parent.gameObject);
            });

            return newUI;
        }

        private static void SetSizeByScreenPercentage(RectTransform _Rt, Vector2 _ScreenPercent, Vector2 _Position, Vector2 _Pivot)
        {
            _Rt.anchorMin = _Rt.anchorMax = _Position;
            _Rt.pivot = _Pivot;

            float width = Screen.width * _ScreenPercent.x;
            float height = Screen.height * _ScreenPercent.y;

            _Rt.sizeDelta = new Vector2(width, height);
        }

        private static GameObject CreateUIElement<T>(string _UIName, string _ElementName, GameObject _Parent, Sprite _Image, Color _Color, Vector2 _MinAnchor, Vector2 _MaxAnchor, bool _IsRaycastTarget, bool _BringToFront) where T : UnityEngine.Component
        {
            GameObject newUIObject = new GameObject(_UIName + "_" + _ElementName);
            T newUIElement = newUIObject.AddComponent<T>();
            newUIElement.transform.SetParent(_Parent.transform, false);

            Image image;
            newUIElement.TryGetComponent<Image>(out image);

            if (image == null)
            {
                image = newUIElement.AddComponent<Image>();
            }

            if (_Image != null)
            {
                image.sprite = _Image;
            }

            image.color = _Color;

            image.rectTransform.anchorMin = _MinAnchor;
            image.rectTransform.anchorMax = _MaxAnchor;
            ResetRectTransform(newUIObject);

            image.raycastTarget = _IsRaycastTarget;

            if (_BringToFront)
            {
                newUIElement.transform.SetAsLastSibling();
            }

            return newUIObject;
        }

        private static void ResetRectTransform(GameObject _Object)
        {
            RectTransform rt = _Object.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        public static GameObject CreateNewTextElement(string _UIName, string _Text, Color _TextColor, Color _HyperlinkColor, int _FontSize, Vector2 _MinAnchor, Vector2 _MaxAnchor, bool _IsRaycastTarget)
        {
            GameObject textObj = new GameObject(_UIName + "_Text");
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = CreateHyperlinks(_Text, _HyperlinkColor);
            text.fontSize = _FontSize;
            text.color = _TextColor;
            text.rectTransform.anchorMin = _MinAnchor;
            text.rectTransform.anchorMax = _MaxAnchor;
            text.richText = true;
            text.raycastTarget = _IsRaycastTarget;

            return textObj;
        }

        private static string CreateHyperlinks(string _Text, Color _Color)
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

        private static List<string> GetSubstringsInBraces(string _Input)
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
}