using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public enum UIAxis
{
    Vertical, Horizontal
}

public class AxisBox : 
    AUIBox
{
    private static Vector2f CalculateSize(UIStyle style, UIAxis axis, IEnumerable<AUIElement> children)
    {
        var size = new Vector2f(0, 0);

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

    protected override void UpdateMinimalSize()
        => MinimalSize = CalculateSize(Host.Style, _axis, _children);

    private readonly List<AUIElement> _children;
    private readonly UIAxis _axis;

    public AxisBox(UIHost host, UIAxis axis, AUIElement[] children) : base(host, CalculateSize(host.Style, axis, children))
    {
        foreach (var child in children)
            child.Parent = this;
        _axis = axis;
        _children = new List<AUIElement>(children);
    }

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
        if (_axis == UIAxis.Horizontal)
            foreach (var child in _children)
            {
                child.Rect = new FloatRect(
                    position.X, position.Y,
                    child.MinimalSize.X, Rect.Height
                );
                position.X += child.MinimalSize.X + Host.Style.AxisBoxSpace;
            }
        else
            foreach (var child in _children)
            {
                child.Rect = new FloatRect(
                    position.X, position.Y,
                    Rect.Width, child.MinimalSize.Y
                );
                position.Y += child.MinimalSize.Y + Host.Style.AxisBoxSpace;
            }
    }
    
    public override void Draw(RenderTarget target)
    {
        foreach (var child in _children)
            Host.DrawStack.Push(child.Draw);
    }
}