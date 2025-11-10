using System.Numerics;
using Raylib_cs;

namespace leditor.UI;

public enum UIAxis
{
    Vertical, Horizontal
}

public class AxisBox(UIHost host, UIAxis axis, AUIElement[] children): 
    AUIBox(host, CalculateSize(host.Style, axis, children))
{
    private static Vector2 CalculateSize(UIStyle style, UIAxis axis, IEnumerable<AUIElement> children)
    {
        var size = Vector2.Zero;

        if (axis == UIAxis.Horizontal)
        {
            foreach (var child in children)
            {
                size.Y = float.Max(size.Y, child.MinimalSize.Y);
                size.X += child.MinimalSize.X + style.AxisBoxSpace;
            }
            size.X -= style.AxisBoxSpace;
        }
        else
        {
            foreach (var child in children)
            {
                size.X = float.Max(size.X, child.MinimalSize.X);
                size.Y += child.MinimalSize.Y + style.AxisBoxSpace;
            }
            size.Y -= style.AxisBoxSpace;
        }

        return size;
    }
    
    public override void UpdateMinimalSize()
        => MinimalSize = CalculateSize(Host.Style, axis, _children);

    private readonly List<AUIElement> _children = new(children);

    public override IEnumerable<AUIElement> GetChildren()
        => _children;

    public override void RemoveChild(AUIElement child)
    {
        child.Parent = null;
        _children.Remove(child);
        UpdateMinimalSize();
    }

    public void AddChild(AUIElement child)
    {
        child.Parent = this;
        _children.Add(child);
        UpdateMinimalSize();
    }
    
    public override void UpdateLayout()
    {
        var position = Rect.Position;
        if (axis == UIAxis.Horizontal)
            foreach (var child in _children)
            {
                child.Rect = new Rectangle(
                    position.X, position.Y,
                    child.MinimalSize.X, Rect.Height
                );
                position.X += child.MinimalSize.X + Host.Style.AxisBoxSpace;
            }
        else
            foreach (var child in _children)
            {
                child.Rect = new Rectangle(
                    position.X, position.Y,
                    Rect.Width, child.MinimalSize.Y
                );
                position.Y += child.MinimalSize.Y + Host.Style.AxisBoxSpace;
            }
    }
    
    public override void Draw()
    {
        foreach (var child in _children)
            Host.DrawStack.Push(child.Draw);
    }
}