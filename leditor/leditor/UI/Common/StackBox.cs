using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class StackBox : AUIBox
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

    private readonly List<AUIElement> _children;

    public override IEnumerable<AUIElement> GetChildren()
        => _children;

    public override void RemoveChild(AUIElement child)
    {
        child.Parent = null;
        _children.Remove(child);
    }

    protected override void UpdateMinimalSize()
        => MinimalSize = GetMinSize(_children, _padding);
    
    public void AddChild(AUIElement child)
    {
        child.Parent = this;
        
        _children.Add(child);
        MinimalSize = new Vector2f(
            float.Max(MinimalSize.X, child.MinimalSize.X),
            float.Max(MinimalSize.Y, child.MinimalSize.Y)
        );
    }

    private UIPadding _padding;

    public StackBox(UIHost host, AUIElement[] children, UIPadding padding = default, bool centerX = false, bool centerY = false) : base(host, GetMinSize(children.AsEnumerable(), padding))
    {
        _centerX = centerX;
        _centerY = centerY;
        _children = new List<AUIElement>(children);
        _padding = padding;

        foreach (var child in _children)
            child.Parent = this;
    }

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

    protected override void UpdateLayout()
    {
        var baseRect = new FloatRect(
            Rect.Left + Padding.Left, 
            Rect.Top + Padding.Top,
            Rect.Width - Padding.Left - Padding.Right, 
            Rect.Height - Padding.Bottom - Padding.Top
        );

        baseRect.Left += baseRect.Width / 2;
        baseRect.Top += baseRect.Height / 2;

        foreach (var child in _children)
        {
            if (_centerX)
                baseRect.Width = child.MinimalSize.X;
            if (_centerY)
                baseRect.Height = child.MinimalSize.Y;

            child.SetRect(new FloatRect(
                baseRect.Left - baseRect.Width / 2,
                baseRect.Top - baseRect.Height / 2,
                baseRect.Width,
                baseRect.Height
            ));
        }
    }

    public override void Draw(RenderTarget target)
    {
        foreach (var child in _children.AsEnumerable().Reverse())
            Host.DrawStack.Push(child.Draw);
    }

    private bool _centerX;
    private bool _centerY;

    public bool CenterX
    {
        get => _centerX;
        set
        {
            _centerX = value;
            UpdateLayout();
        }
    }
    
    public bool CenterY
    {
        get => _centerY;
        set
        {
            _centerY = value;
            UpdateLayout();
        }
    }
}