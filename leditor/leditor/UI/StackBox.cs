using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class StackBox(UIHost host, AUIElement[] children, UIPadding padding = default) : AUIBox(host, GetMinSize(children.AsEnumerable(), padding))
{
    private static Vector2f GetMinSize(IEnumerable<AUIElement> children, UIPadding padding)
    {
        var size = new Vector2f();
        foreach (var child in children)
        {
            size.X = float.Max(size.X, child.MinimalSize.X);
            size.Y = float.Max(size.Y, child.MinimalSize.Y);
        }

        size.X += padding.Left + padding.Right;
        size.Y += padding.Top + padding.Bottom;
        
        return size;
    }

    private readonly List<AUIElement> _children = new(children);

    public override IEnumerable<AUIElement> GetChildren()
        => children;

    public override void RemoveChild(AUIElement child)
    {
        child.Parent = null;
        _children.Remove(child);
    }

    public override void UpdateMinimalSize()
        => MinimalSize = GetMinSize(_children, _padding);
    
    public void AddChild(AUIElement child)
    {
        child.Parent = this;
        Host.NeedLayoutUpdate = true;
        
        _children.Add(child);
        MinimalSize = new Vector2f(
            float.Max(MinimalSize.X, child.MinimalSize.X),
            float.Max(MinimalSize.Y, child.MinimalSize.Y)
        );
    }

    private UIPadding _padding = padding;

    public UIPadding Padding
    {
        get => _padding;
        set
        {
            _padding = value;
            MinimalSize = new Vector2f(
                MinimalSize.X + _padding.Left + _padding.Right - value.Left - value.Right,
                MinimalSize.Y + _padding.Top + _padding.Bottom - value.Top - value.Bottom
            );
        }
    }
    
    public override void UpdateLayout()
    {
        var rect = new FloatRect(
            Rect.Left + Padding.Left, 
            Rect.Top + Padding.Top,
            Rect.Width - Padding.Left - Padding.Right, 
            Rect.Height - Padding.Bottom - Padding.Top
        );
        
        foreach (var child in _children)
        {
            child.Rect = rect;
        }
    }

    public override void Draw(RenderTarget target)
    {
        foreach (var child in _children.AsEnumerable().Reverse())
            Host.DrawStack.Push(child.Draw);
    }
}