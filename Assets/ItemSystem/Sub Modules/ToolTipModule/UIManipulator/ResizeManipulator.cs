using UnityEngine;
using UnityEngine.UIElements;

public class ResizeManipulator : PointerManipulator
{
    private VisualElement m_Window;
    private Vector2 m_StartMousePosition;
    private Vector2 m_StartWindowPosition;
    private Vector2 m_StartSize;
    private Vector2 m_ResizeDir;
    private System.Func<VisualElement> m_GetWindow;

    public ResizeManipulator(System.Func<VisualElement> _GetWindow)
    {
        m_GetWindow = _GetWindow;
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
    }

    private void OnPointerDown(PointerDownEvent _Event)
    {
        m_Window = m_GetWindow.Invoke();
        if (m_Window == null)
        {
            return;
        }

        m_StartMousePosition = _Event.position;
        m_StartSize = new Vector2(m_Window.resolvedStyle.width, m_Window.resolvedStyle.height);
        m_StartWindowPosition = new Vector2(
           m_Window.resolvedStyle.left,
           m_Window.resolvedStyle.top
        );

        Rect windowRect = m_Window.worldBound;
        Rect handleRect = target.worldBound;

        Vector2 handleCenter = new Vector2(
            handleRect.center.x,
            handleRect.center.y
        );

        Vector2 windowCenter = new Vector2(
            windowRect.center.x,
            windowRect.center.y
        );

        Vector2 dir = handleCenter - windowCenter;

        m_ResizeDir = new Vector2(
            Mathf.Abs(dir.x) < 1f ? 0 : Mathf.Sign(dir.x),
            Mathf.Abs(dir.y) < 1f ? 0 : Mathf.Sign(dir.y)
        );

        target.CapturePointer(_Event.pointerId);
        _Event.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent _Event)
    {
        if (target.HasPointerCapture(_Event.pointerId))
        {
            m_Window = m_GetWindow.Invoke();
            if (m_Window == null)
            {
                return;
            }

            Vector2 mouseDelta = (Vector2)_Event.position - m_StartMousePosition;

            Vector2 deltaSize = new Vector2(
                mouseDelta.x * m_ResizeDir.x,
                mouseDelta.y * m_ResizeDir.y
            );

            Vector2 newSize = m_StartSize + deltaSize;

            newSize.x = Mathf.Max(newSize.x, m_Window.style.minWidth.value.value);
            newSize.y = Mathf.Max(newSize.y, m_Window.style.minHeight.value.value);

            Vector2 actualChange = newSize - m_StartSize;

            if (actualChange.x != 0 || actualChange.y != 0)
            {
                Vector2 offset = new Vector2(
               (m_ResizeDir.x < 0) ? actualChange.x : 0,
               (m_ResizeDir.y < 0) ? actualChange.y : 0
                );

                m_Window.style.width = newSize.x;
                m_Window.style.height = newSize.y;

                m_Window.style.left = m_StartWindowPosition.x - offset.x;
                m_Window.style.top = m_StartWindowPosition.y - offset.y;
            }

            _Event.StopPropagation();
        }
    }

    private void OnPointerUp(PointerUpEvent _Event)
    {
        if (target.HasPointerCapture(_Event.pointerId))
        {
            target.ReleasePointer(_Event.pointerId);
            _Event.StopPropagation();
        }
    }
}
