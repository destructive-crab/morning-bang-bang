using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace leditor.UI;

public delegate void MoveAction(Vector2f oldPosition, Vector2f newPosition);
public class ClickArea(FloatRect rect, bool overlay = true)
{
    public FloatRect Rect = rect;
    public bool Overlay = overlay;
    
    public Action? OnClick;
    public Action? OnHover;
    public Action? OnUnhover;
    public MoveAction? OnMove;

    public bool IsHovered;
    public bool IsGrabbed;
}

public class ClickAreasController
{
    private readonly List<ClickArea> _areas = [];
    private Vector2f _mousePosition = new(1, 1);
    private bool _isPressedOld;

    public void Update(RenderWindow window)
    {
        var isPressed = Mouse.IsButtonPressed(Mouse.Button.Left);
        var isClicked = isPressed && !_isPressedOld;
        
        var newPositionInts = Mouse.GetPosition(window);
        var newPosition = new Vector2f(newPositionInts.X, newPositionInts.Y);
        using var areaIter = _areas
            .AsEnumerable()
            .Reverse()
            .GetEnumerator();
        
        while (areaIter.MoveNext())
        {
            var area = areaIter.Current;
            var inner = _mousePosition - area.Rect.Position;
            var newIsHovered =
                inner.X >= 0 && inner.X <= area.Rect.Width &&
                inner.Y >= 0 && inner.Y <= area.Rect.Height;

            if (newIsHovered)
            {
                if (!area.IsHovered)
                    area.OnHover?.Invoke();
            
                if (isClicked)
                {
                    area.IsGrabbed = true;
                    area.OnClick?.Invoke();
                }
            }
            else if (area.IsHovered)
                area.OnUnhover?.Invoke();

            area.IsGrabbed = area.IsGrabbed && isPressed;
            if (area.IsGrabbed && _mousePosition != newPosition)
                area.OnMove?.Invoke(_mousePosition, newPosition);
            
            area.IsHovered = newIsHovered;
            
            if (area.IsHovered && area.Overlay)
                break;
        }

        while (areaIter.MoveNext())
        {
            var area = areaIter.Current;
            if (!area.IsHovered) continue;
            area.IsHovered = false;
            area.OnUnhover?.Invoke();
        }

        _mousePosition = newPosition;
        _isPressedOld = isPressed;
    }

    public void AddArea(ClickArea area)
        => _areas.Add(area);

    public void RemoveArea(ClickArea area)
        => _areas.Remove(area);
}