using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public struct Anchor(FloatRect baseRect, FloatRect relative)
{
    public FloatRect BaseRect = baseRect;
    public FloatRect Relative = relative;
}

public class AnchorBox(UIHost host) : AUIBox(host, new Vector2f(0, 0))
{
    private readonly List<(Anchor, AUIElement)> _children = [];

    protected override void UpdateLayout()
    {
        foreach (var (anchor, child) in _children)
        {
            var rect = anchor.BaseRect;
            rect.Left += Rect.Left + anchor.Relative.Left * Rect.Width;
            rect.Top += Rect.Top + anchor.Relative.Top * Rect.Height;
            rect.Width += anchor.Relative.Width * Rect.Width;
            rect.Height += anchor.Relative.Height * Rect.Height;
            child.SetRect(rect);
        }
    }
    
    public override IEnumerable<AUIElement> GetChildren()
        => _children
            .Select(tuple => tuple.Item2)
            .AsEnumerable();

    public override void RemoveChild(AUIElement child)
    {
        child.Parent = null;
        (Anchor, AUIElement) match = _children.FirstOrDefault(tuple => tuple.Item2 == child);
        if (match.Item2 != null)
        {
            _children.Remove(match);
        }
        UpdateLayout();
    }

    public AnchorBox AddChild(Anchor anchor, AUIElement child)
    {
        child.Parent = this;
        _children.Add((anchor, child));
        
        UpdateLayout();
        return this;
    }

    protected override void UpdateMinimalSize() { }

    public override void Draw(RenderTarget target)
    {
        foreach (var child in _children.AsEnumerable().Reverse())
        {
            child.Item2.Draw(target);
        }
    }
}