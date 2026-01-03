using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace leditor.UI;

public delegate void MoveAction(Vector2f oldPosition, Vector2f newPosition);
public class ClickArea(FloatRect rect, bool overlay = true)
{
    public FloatRect Rect = rect;
    public bool Overlay = overlay;
    
    public Action? OnRightMouseButtonClick;
    public Action? OnRightMouseButtonReleased;
    public Action? OnHover;
    public Action? OnUnhover;
    public MoveAction? OnMove;

    public bool IsHovered;
    public bool IsGrabbed;
}

public class ClickAreasController
{
    private Vector2f _mousePositionOld = new(1, 1);
    private Vector2f _mousePosition;
    
    private bool _isPressedOld;
    private bool _isPressed;
    private bool _isClicked;

    private bool _inView;
    private Stack<bool> _inViewStack = new();

    private bool _isOverlayed;

    public void Begin(Vector2f mousePosition)
    {
        _isOverlayed = false;
        _inView = true;
        _isPressed = Mouse.IsButtonPressed(Mouse.Button.Left);
        _isClicked = _isPressed && !_isPressedOld;
        
        _mousePosition = mousePosition;
    }
    
    public void End()
    {
        _mousePositionOld = _mousePosition;
        _isPressedOld = _isPressed;
    }

    public void SetViewport(FloatRect rect)
    {
        _inViewStack.Push(_inView);
        _inView = Utils.PointInRect(rect, _mousePositionOld);
    }

    public void PopViewport()
    {
        _inView = _inViewStack.Pop();
    }

    public void Process(ClickArea area)
    {
        if (_isOverlayed || !_inView)
        {
            if (!area.IsHovered) return;
            area.IsHovered = false;
            area.OnUnhover?.Invoke();
            
            return;
        }
        
        var newIsHovered = Utils.PointInRect(area.Rect, _mousePositionOld);
        
        if (newIsHovered)
        {
            if (!area.IsHovered)
                area.OnHover?.Invoke();
            
            if (_isClicked)
            {
                area.IsGrabbed = true;
                area.OnRightMouseButtonClick?.Invoke();
            }
        }
        else if (area.IsHovered)
            area.OnUnhover?.Invoke();

        area.IsGrabbed = area.IsGrabbed && _isPressed;
        if (area.IsGrabbed && _mousePositionOld != _mousePosition)
            area.OnMove?.Invoke(_mousePositionOld, _mousePosition);
        
        if(!_isPressed && _isPressedOld && newIsHovered) area.OnRightMouseButtonReleased?.Invoke();
            
        area.IsHovered = newIsHovered;
        _isOverlayed = area.IsHovered && area.Overlay;
    }
}