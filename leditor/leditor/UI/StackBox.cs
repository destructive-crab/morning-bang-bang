using System.Numerics;
using Raylib_cs;

namespace leditor.UI;

public class StackBox(UIHost host, AUIElement[] children, UIPadding padding = default) : AUIBox(host, GetMinSize(children.AsEnumerable(), padding))
{
    private static Vector2 GetMinSize(IEnumerable<AUIElement> children, UIPadding padding)
    {
        var size = Vector2.Zero;
        foreach (var child in children)
        {
            size.X = float.Min(size.X, child.MinimalSize.X);
            size.Y = float.Min(size.Y, child.MinimalSize.Y);
        }

        size.X -= padding.Left + padding.Right;
        size.Y -= padding.Top + padding.Bottom;
        
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
        MinimalSize = new Vector2(
            float.Min(MinimalSize.X, child.MinimalSize.X),
            float.Min(MinimalSize.Y, child.MinimalSize.Y)
        );
    }

    private UIPadding _padding = padding;

    public UIPadding Padding
    {
        get => _padding;
        set
        {
            _padding = value;
            MinimalSize = new Vector2(
                MinimalSize.X + _padding.Left + _padding.Right - value.Left - value.Right,
                MinimalSize.Y + _padding.Top + _padding.Bottom - value.Top - value.Bottom
            );
        }
    }
    
    public override void UpdateLayout()
    {
        var rect = new Rectangle(
            Rect.X + Padding.Left, 
            Rect.Y + Padding.Top,
            Rect.Width - Padding.Left - Padding.Right, 
            Rect.Height - Padding.Bottom - Padding.Top
        );
        
        foreach (var child in _children)
        {
            child.Rect = rect;
        }
    }

    public override void Draw()
    {
        foreach (var child in _children)
            Host.DrawStack.Push(child.Draw);
    }
}