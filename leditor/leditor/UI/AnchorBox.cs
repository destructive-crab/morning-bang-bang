using System.Numerics;
using leditor.root;
using Raylib_cs;

namespace leditor.UI;

public struct AnchorBoxChild(Rectangle baseRect, Rectangle relative, AUIElement element)
{
    public Rectangle BaseRect = baseRect;
    public Rectangle Relative = relative;
    public readonly AUIElement Element = element;
}

public class AnchorBox(UIHost host) : AUIElement(host, Vector2.Zero)
{
    public readonly List<AnchorBoxChild> Children = [];
    
    internal override void OnRectUpdate()
    {
        foreach (var child in Children)
        {
            var rect = child.BaseRect;
            rect.X += Rect.X + child.Relative.X * Rect.Width;
            rect.Y += Rect.Y + child.Relative.Y * Rect.Height;
            rect.Width += child.Relative.Width * Rect.Width;
            rect.Height += child.Relative.Height * Rect.Height;
            child.Element.Rect = rect;
        }
    }

    public override void Draw()
    {
        foreach (var child in Children)
            Host.DrawQueue.Enqueue(child.Element);
    }
}