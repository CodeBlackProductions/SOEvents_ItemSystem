using ItemSystem.MainModule;
using ItemSystem.SubModules;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class UIController : MonoBehaviour
{
    private static UIController m_instance;

    public static UIController Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<UIController>();

                if (m_instance == null)
                {
                    GameObject singletonObject = new GameObject("UIController");
                    m_instance = singletonObject.AddComponent<UIController>();
                }
            }

            return m_instance;
        }
    }

    [SerializeField] private UIDocument m_UiDocument;

    private VisualElement m_TempPopup;
    private VisualElement m_Root;
    private List<string> m_LinksOpenAsFixedWindow = new List<string>();

    private void Start()
    {
        if (m_UiDocument == null)
        {
            Debug.LogError("UIDocument not assigned!");
            return;
        }

        m_Root = m_UiDocument.rootVisualElement;
    }

    public void LoadTooltip(SO_Item _Item) 
    {
        string header = UIToolTipRegistry.Instance.RetrieveTooltip(_Item.ItemName + "/Header");

        string content = UIToolTipRegistry.Instance.RetrieveTooltip(_Item.ItemName + "/Body");

        UITKFactory.CreateNewUIWindow(
            "Test",
            new Vector2(0.5f * Screen.width, 0.5f * Screen.height),
            header,
            content,
            Color.white,
            Color.blue,
            Color.red,
            30,
            m_UiDocument,
            true,
            OnHyperlinkClicked,
            OnHyperlinkHovered,
            OnHyperlinkStopHovered,
            "",
            OnWindowClosed

        );
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

            if (m_TempPopup != null)
            {
                m_Root.Remove(m_TempPopup);
                m_TempPopup = null;
            }

            VisualElement newWindow = UITKFactory.CreateNewUIWindow(
                evt.linkText,
                evt.position,
                evt.linkText,
                content,
                Color.white,
                Color.blue,
                Color.red,
                30,
                m_UiDocument,
                true,
                OnHyperlinkClicked,
                OnHyperlinkHovered,
                OnHyperlinkStopHovered,
                evt.linkID,
                OnWindowClosed
            );

            m_Root.Add(newWindow);
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

            Vector3 pos = evt.position + new Vector3(5, 5, 0);

            m_TempPopup = UITKFactory.CreateNewUIWindow(
                evt.linkID,
                pos,
                evt.linkText,
                content,
                Color.white,
                Color.blue,
                Color.red,
                30,
                m_UiDocument
            );
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

    private void OnWindowClosed(string _LinkID)
    {
        if (m_LinksOpenAsFixedWindow.Contains(_LinkID))
        {
            m_LinksOpenAsFixedWindow.Remove(_LinkID);
        }
    }
}