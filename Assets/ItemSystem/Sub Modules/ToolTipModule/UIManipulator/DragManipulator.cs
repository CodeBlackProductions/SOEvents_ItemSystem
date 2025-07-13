using UnityEngine;
using UnityEngine.UIElements;

public class DragManipulator : PointerManipulator
{
    private Vector2 m_StartMousePosition;
    private Vector2 m_StartElementPosition;
    private VisualElement m_Window;
    private System.Func<VisualElement> m_GetWindow;

    public DragManipulator(System.Func<VisualElement> _GetWindow)
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

        m_StartElementPosition = new Vector2(
            m_Window.resolvedStyle.left,
            m_Window.resolvedStyle.top
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

            Vector2 delta = (Vector2)_Event.position - m_StartMousePosition;

            float newX = m_StartElementPosition.x + delta.x;
            float newY = m_StartElementPosition.y + delta.y;

            m_Window.style.left = newX;
            m_Window.style.top = newY;

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
