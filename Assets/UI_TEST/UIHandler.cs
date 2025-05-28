using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIHandler : MonoBehaviour
{
    public static UIHandler Instance { get; private set; }

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

    private int lastHoveredLinkIndex = -1;
    private TextMeshProUGUI lastHoveredText = null;

    private void Update()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        TextMeshProUGUI hoveredText = null;
        foreach (var result in results)
        {
            hoveredText = result.gameObject.GetComponent<TextMeshProUGUI>();
            if (hoveredText != null)
                break;
        }

        int linkIndex = -1;
        if (hoveredText != null)
        {
            linkIndex = TMP_TextUtilities.FindIntersectingLink(hoveredText, Input.mousePosition, null);
        }

        if (hoveredText != lastHoveredText || linkIndex != lastHoveredLinkIndex)
        {
            if (lastHoveredText != null && lastHoveredLinkIndex != -1)
            {
                TMP_LinkInfo linkInfo = lastHoveredText.textInfo.linkInfo[lastHoveredLinkIndex];
                OnLinkHoverEnd(linkInfo.GetLinkID());
            }
            if (hoveredText != null && linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = hoveredText.textInfo.linkInfo[linkIndex];
                OnLinkHoverStart(linkInfo.GetLinkID());
            }
        }

        lastHoveredText = hoveredText;
        lastHoveredLinkIndex = linkIndex;
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                var tmp = result.gameObject.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    int linkIndex = TMP_TextUtilities.FindIntersectingLink(tmp, Input.mousePosition, null);
                    if (linkIndex != -1)
                    {
                        TMP_LinkInfo linkInfo = tmp.textInfo.linkInfo[linkIndex];
                        string linkId = linkInfo.GetLinkID();

                        if (Input.GetMouseButtonDown(0))
                            OnLinkLeftClick(linkId);
                        else if (Input.GetMouseButtonDown(1))
                            OnLinkRightClick(linkId);

                        break;
                    }
                }
            }
        }
    }

    private void OnLinkHoverStart(string linkId)
    {
        Debug.Log($"Hover Start: {linkId}");
        // Hier gewünschte Logik für Hover-Start
    }

    private void OnLinkHoverEnd(string linkId)
    {
        Debug.Log($"Hover End: {linkId}");
        // Hier gewünschte Logik für Hover-Ende
    }

    private void OnLinkLeftClick(string linkId)
    {
        string text = UIToolTipRegistry.Instance.RetrieveTooltip(linkId);
        UIFactory.Instance.CreateNewUI($"{linkId}", text, Color.white, Color.blue, 36);
    }

    private void OnLinkRightClick(string linkId)
    {
        Debug.Log($"Right Click: {linkId}");
        // Hier gewünschte Logik für Rechtsklick
    }
}