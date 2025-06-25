using ItemSystem.SubModules;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class RuntimeUI : MonoBehaviour
{
    public UIDocument uiDocument;

    private VisualElement m_TempPopup;
    private VisualElement m_Root;
    private List<string> m_LinksOpenAsFixedWindow = new List<string>();

    private void Start()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument not assigned!");
            return;
        }

        m_Root = uiDocument.rootVisualElement;

        string header = UIToolTipRegistry.Instance.RetrieveTooltip("Item/OnHit/MeleeBasic/Header");
        header = UIFactory.ImportValues(header, Color.red);
        header = UIFactory.CreateHyperlink(header, Color.blue);

        string content = UIToolTipRegistry.Instance.RetrieveTooltip("Item/OnHit/MeleeBasic/Body");
        content = UIFactory.ImportValues(content, Color.red);
        content = UIFactory.CreateHyperlink(content, Color.blue);

        VisualElement popupWindow = CreatePopupWindow(header, content, Vector2.zero);
        popupWindow.Focus();

        m_Root.Add(popupWindow);
    }

    private VisualElement CreatePopupWindow(string _Header, string _Content, Vector2 _ScreenOffset)
    {
        UnityEngine.UIElements.PopupWindow popupWindow = new UnityEngine.UIElements.PopupWindow();
        popupWindow.enableRichText = true;

        popupWindow.text = _Header;
        popupWindow.style.fontSize = 36;
        popupWindow.style.alignContent = Align.Center;
        popupWindow.style.backgroundColor = Color.gray;
        popupWindow.style.width = Length.Percent(50);
        popupWindow.style.height = Length.Percent(50);
        popupWindow.style.alignSelf = Align.Center;
        popupWindow.style.position = Position.Absolute;
        popupWindow.style.translate = new StyleTranslate(new Translate(Length.Percent(0 + _ScreenOffset.x), Length.Percent(50 + _ScreenOffset.y)));

        TextElement popupWindowText = new TextElement();
        popupWindowText.enableRichText = true;

        popupWindowText.text = _Content;
        popupWindowText.style.width = Length.Percent(100);
        popupWindowText.style.height = Length.Percent(100);
        popupWindowText.style.fontSize = 30;
        popupWindowText.style.marginTop = Length.Percent(3);
        popupWindowText.style.paddingTop = Length.Percent(1);
        popupWindowText.style.paddingBottom = Length.Percent(1);
        popupWindowText.style.paddingRight = Length.Percent(1);
        popupWindowText.style.paddingLeft = Length.Percent(1);
        popupWindowText.style.borderTopColor = Color.white;
        popupWindowText.style.borderTopWidth = 5;
        popupWindowText.style.backgroundColor = new Color(0, 0, 0, 0.5f);
        popupWindowText.RegisterCallback<PointerUpLinkTagEvent>(OnHyperlinkClicked);
        popupWindowText.RegisterCallback<PointerOverLinkTagEvent>(OnHyperlinkHovered);
        popupWindowText.RegisterCallback<PointerOutLinkTagEvent>(OnHyperlinkStopHovered);

        popupWindow.Add(popupWindowText);

        return popupWindow;
    }

    private VisualElement CreateFixedWindow(string _Header, string _Content, Vector2 _ScreenOffset)
    {
        VisualElement contentContainer = new VisualElement();

        contentContainer.style.alignContent = Align.Center;
        contentContainer.style.backgroundColor = Color.gray;
        contentContainer.style.width = Length.Percent(50);
        contentContainer.style.height = Length.Percent(50);
        contentContainer.style.alignSelf = Align.Center;
        contentContainer.style.position = Position.Absolute;
        contentContainer.style.translate = new StyleTranslate(new Translate(Length.Percent(0 + _ScreenOffset.x), Length.Percent(50 + _ScreenOffset.y)));

        return contentContainer;
    }

    private void OnHyperlinkClicked(PointerUpLinkTagEvent evt)
    {
        if (evt.button == 0)
        {
            if (m_LinksOpenAsFixedWindow.Contains(evt.linkID))
            {
                return;
            }
            string content = UIToolTipRegistry.Instance.RetrieveTooltip(evt.linkID);
            content = UIFactory.ImportValues(content, Color.red);
            content = UIFactory.CreateHyperlink(content, Color.blue);

            if (m_TempPopup != null)
            {
                m_Root.Remove(m_TempPopup);
                m_TempPopup = null;
            }

            m_Root.Add(CreateFixedWindow(evt.linkText, content, new Vector2(50, -50)));
            m_LinksOpenAsFixedWindow.Add(evt.linkID);
        }

        if (evt.button == 1)
        {
            Debug.Log($"Righ Clicked the link for {evt.linkID}");
        }
    }

    private void OnHyperlinkHovered(PointerOverLinkTagEvent evt)
    {
        if (m_LinksOpenAsFixedWindow.Contains(evt.linkID))
        {
            return;
        }

        if (m_TempPopup.IsUnityNull())
        {
            string content = UIToolTipRegistry.Instance.RetrieveTooltip(evt.linkID);
            content = UIFactory.ImportValues(content, Color.red);
            content = UIFactory.CreateHyperlink(content, Color.blue);

            m_TempPopup = CreatePopupWindow(evt.linkText, content, new Vector2(50, -50));
            m_Root.Add(m_TempPopup);
        }
    }

    private void OnHyperlinkStopHovered(PointerOutLinkTagEvent evt)
    {
        if (!m_TempPopup.IsUnityNull())
        {
            m_Root.Remove(m_TempPopup);
            m_TempPopup = null;
        }
    }
}