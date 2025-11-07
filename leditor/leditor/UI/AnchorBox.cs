using System.Numerics;
using leditor.root;
using Raylib_cs;

namespace leditor.UI;

public struct Anchor(Rectangle baseRect, Rectangle relative)
{
    public Rectangle BaseRect = baseRect;
    public Rectangle Relative = relative;
}

public class AnchorBox(UIHost host) : AUIBox(host, Vector2.Zero)
{
    private readonly List<(Anchor, AUIElement)> _children = [];
    
    public override void UpdateLayout()
    {
        foreach (var (anchor, child) in _children)
        {
            var rect = anchor.BaseRect;
            rect.X += Rect.X + anchor.Relative.X * Rect.Width;
            rect.Y += Rect.Y + anchor.Relative.Y * Rect.Height;
            rect.Width += anchor.Relative.Width * Rect.Width;
            rect.Height += anchor.Relative.Height * Rect.Height;
            child.Rect = rect;
        }
    }

    public override IEnumerable<AUIElement> GetChildren()
        => _children
            .Select(tuple => tuple.Item2)
            .AsEnumerable();

    public override void RemoveChild(AUIElement child)
    {
        _children.Remove(
            _children.First(tuple => tuple.Item2 == child)
        );
    }

    public void AddChild(Anchor anchor, AUIElement child)
    {
        Host.NeedLayoutUpdate = true;
        _children.Add((anchor, child));
    }
    
    public override void UpdateMinimalSize() { }

    public override void Draw()
    {
        foreach (var child in _children)
            Host.DrawStack.Push(child.Item2.Draw);
    }
}