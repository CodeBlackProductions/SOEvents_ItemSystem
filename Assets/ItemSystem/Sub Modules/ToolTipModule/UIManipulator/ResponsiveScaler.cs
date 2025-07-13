using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResponsiveScaler
{
    private Vector2? m_InitialSize = null;
    private Dictionary<VisualElement, ElementBaseStyle> m_OriginalStyles = new();

    public void Setup(VisualElement window)
    {
        window.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            float width = window.resolvedStyle.width;
            float height = window.resolvedStyle.height;

            if (m_InitialSize == null)
                m_InitialSize = new Vector2(width, height);

            Vector2 initial = m_InitialSize.Value;
            float scaleX = width / initial.x;
            float scaleY = height / initial.y;
            float scale = Mathf.Min(scaleX, scaleY);

            ApplyScale(window, scale);
        });
    }

    private void ApplyScale(VisualElement element, float scale)
    {
        // Cache original styles once
        if (!m_OriginalStyles.ContainsKey(element))
        {
            m_OriginalStyles[element] = new ElementBaseStyle
            {
                fontSize = element.resolvedStyle.fontSize,
                paddingTop = element.resolvedStyle.paddingTop,
                paddingBottom = element.resolvedStyle.paddingBottom,
                paddingLeft = element.resolvedStyle.paddingLeft,
                paddingRight = element.resolvedStyle.paddingRight,
                width = element.resolvedStyle.width,
                height = element.resolvedStyle.height
            };
        }

        var baseStyle = m_OriginalStyles[element];

        // Apply to Label
        if (element is Label label)
        {
            label.style.fontSize = baseStyle.fontSize * scale;
        }

        // Apply to Button
        if (element is Button button)
        {
            button.style.fontSize = baseStyle.fontSize * scale;
            button.style.paddingTop = baseStyle.paddingTop * scale;
            button.style.paddingBottom = baseStyle.paddingBottom * scale;
            button.style.paddingLeft = baseStyle.paddingLeft * scale;
            button.style.paddingRight = baseStyle.paddingRight * scale;
        }

        // Apply to Image
        if (element is Image image)
        {
            image.style.width = baseStyle.width * scale;
            image.style.height = baseStyle.height * scale;
        }

        // Recursively scale children
        foreach (var child in element.Children())
        {
            ApplyScale(child, scale);
        }
    }

    private class ElementBaseStyle
    {
        public float fontSize;
        public float paddingTop, paddingBottom, paddingLeft, paddingRight;
        public float width, height;
    }
}