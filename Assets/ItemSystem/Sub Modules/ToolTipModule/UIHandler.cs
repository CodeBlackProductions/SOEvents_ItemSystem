using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ItemSystem.SubModules
{
    /// <summary>
    /// Singleton class to handle UI interactions related to TextMeshPro links / Hyperlinks.
    /// </summary>
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

        private int m_LastHoveredLinkIndex = -1;
        private TextMeshProUGUI m_LastHoveredText = null;

        private void Update()
        {
            TextMeshProUGUI hoveredText = GetTopmostValidTMP();

            int linkIndex = -1;
            if (hoveredText != null)
            {
                linkIndex = TMP_TextUtilities.FindIntersectingLink(hoveredText, Input.mousePosition, null);
            }

            if (hoveredText != m_LastHoveredText || linkIndex != m_LastHoveredLinkIndex)
            {
                if (m_LastHoveredText != null && m_LastHoveredLinkIndex != -1)
                {
                    TMP_LinkInfo linkInfo = m_LastHoveredText.textInfo.linkInfo[m_LastHoveredLinkIndex];
                    OnLinkHoverEnd(linkInfo.GetLinkID());
                }
                if (hoveredText != null && linkIndex != -1)
                {
                    TMP_LinkInfo linkInfo = hoveredText.textInfo.linkInfo[linkIndex];
                    OnLinkHoverStart(linkInfo.GetLinkID());
                }
            }

            m_LastHoveredText = hoveredText;
            m_LastHoveredLinkIndex = linkIndex;
        }

        private void LateUpdate()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                TextMeshProUGUI clickedText = GetTopmostValidTMP();

                if (clickedText != null)
                {
                    int linkIndex = TMP_TextUtilities.FindIntersectingLink(clickedText, Input.mousePosition, null);
                    if (linkIndex != -1)
                    {
                        TMP_LinkInfo linkInfo = clickedText.textInfo.linkInfo[linkIndex];
                        string linkId = linkInfo.GetLinkID();

                        if (Input.GetMouseButtonDown(0))
                            OnLinkLeftClick(linkId);
                        else if (Input.GetMouseButtonDown(1))
                            OnLinkRightClick(linkId);
                    }
                }
            }
        }

        private TextMeshProUGUI GetTopmostValidTMP()
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                if (result.gameObject.TryGetComponent<TextMeshProUGUI>(out var tmp))
                {
                    return tmp;
                }

                if (result.gameObject.TryGetComponent<Graphic>(out var graphic))
                {
                    break;
                }
            }

            return null;
        }

        private void OnLinkHoverStart(string linkId)
        {
            Debug.Log($"Hover Start: {linkId}");
            // Hier gew�nschte Logik f�r Hover-Start
        }

        private void OnLinkHoverEnd(string linkId)
        {
            Debug.Log($"Hover End: {linkId}");
            // Hier gew�nschte Logik f�r Hover-Ende
        }

        private void OnLinkLeftClick(string linkId)
        {
            string text = UIToolTipRegistry.Instance.RetrieveTooltip(linkId);
            UIFactory.CreateNewUIWindow($"{linkId}", Input.mousePosition, new Vector2(0f, 0f), new Vector2(0.75f, 0.75f), linkId, text, Color.white, Color.blue, 36);
        }

        private void OnLinkRightClick(string linkId)
        {
            Debug.Log($"Right Click: {linkId}");
            // Hier gew�nschte Logik f�r Rechtsklick
        }
    }
}