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

    public bool IsHovered;
    public bool IsGrabbed;
}

public class ClickAreasController
{
    private readonly List<ClickArea> _areas = [];
    private Vector2 _mousePosition = Vector2.One;

    public void Update()
    {
        var newPosition = Raylib.GetMousePosition();
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
            
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    area.IsGrabbed = true;
                    area.OnClick?.Invoke();
                }
            }
            else if (area.IsHovered)
                area.OnUnhover?.Invoke();

            area.IsGrabbed = area.IsGrabbed&& Raylib.IsMouseButtonDown(MouseButton.Left);
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
    }

    public void AddArea(ClickArea area)
        => _areas.Add(area);

    public void RemoveArea(ClickArea area)
        => _areas.Remove(area);
}