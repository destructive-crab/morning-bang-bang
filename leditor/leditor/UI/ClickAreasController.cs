using System.Numerics;
using Raylib_cs;

namespace leditor.UI;

public delegate void MoveAction(Vector2 oldPosition, Vector2 newPosition);
public class ClickArea(Rectangle rect, bool overlay = true)
{
    public Rectangle Rect = rect;
    public bool Overlay = overlay;
    
    public Action? OnClick;
    public Action? OnHover;
    public Action? OnUnhover;
    public MoveAction? OnMove;
    
    public bool IsHovered { get; private set; }
    private bool _isGrabbed;

    public void Update(Vector2 mousePosition)
    {
        var inner = mousePosition - Rect.Position;
        var newIsHovered =
            inner.X >= 0 && inner.X <= Rect.Width &&
            inner.Y >= 0 && inner.Y <= Rect.Height;

        if (newIsHovered)
        {
            if (!IsHovered)
                OnHover?.Invoke();
            
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                _isGrabbed = true;
                OnClick?.Invoke();
            }
        }
        else if (IsHovered)
            OnUnhover?.Invoke();

        _isGrabbed = _isGrabbed&& Raylib.IsMouseButtonDown(MouseButton.Left);
        var newPosition = Raylib.GetMousePosition();
        if (_isGrabbed && mousePosition != newPosition)
            OnMove?.Invoke(mousePosition, newPosition);
            
        IsHovered = newIsHovered;
    }

}

public class ClickAreasController
{
    private readonly List<ClickArea> _areas = [];
    private Vector2 _mousePosition = Vector2.One;

    public void Update()
    {
        var newPosition = Raylib.GetMousePosition();
        foreach (var area in _areas)
        {
            area.Update(_mousePosition);
            if (area.IsHovered && area.Overlay)
                break;
        }
        
        _mousePosition = newPosition;
    }

    public void AddArea(ClickArea area)
        => _areas.Add(area);

    public void RemoveArea(ClickArea area)
        => _areas.Remove(area);
}