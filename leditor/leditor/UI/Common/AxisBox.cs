using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public enum UIAxis
{
    Vertical, Horizontal
}

public class AxisBox : AUIBox
{
    private static Vector2f CalculateSize(UIStyle style, UIAxis axis, IEnumerable<AUIElement> children)
    {
        var size = new Vector2f(0, 0);

        if (axis == UIAxis.Horizontal)
        {
            foreach (var child in children)
            {
                size.Y = float.Max(size.Y, child.MinimalSize.Y);
                size.X += child.MinimalSize.X + style.AxisBoxSpace();
            }
            
            size.X -= style.AxisBoxSpace();
        }
        else
        {
            foreach (var child in children)
            {
                size.X = float.Max(size.X, child.MinimalSize.X);
                size.Y += child.MinimalSize.Y + style.AxisBoxSpace();
            }
            size.Y -= style.AxisBoxSpace();
        }

        return size;
    }

    private readonly List<AUIElement> _children;
    private readonly UIAxis _axis;
    private readonly bool FitRect;

    public AxisBox(UIHost host, UIAxis axis, params AUIElement[] children) : this(host, axis, false, children) { }
    public AxisBox(UIHost host, UIAxis axis, bool fitRect = false, params AUIElement[] children) : base(host, CalculateSize(host.Style, axis, children))
    {
        FitRect = fitRect;
        foreach (var child in children)
        {
            child.Parent = this;
        }

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

    public AUIElement AddChild(AUIElement child)
    {
        child.Parent = this;
        _children.Add(child);
        UpdateMinimalSize();
        return child;
    }
    
    public override void UpdateLayout()
    {
        Vector2f position = Rect.Position;
        
        if (_axis == UIAxis.Horizontal)
        {
            if (FitRect)
            {
                int w = (int)Rect.Width;

                if (_children.Count == 1)
                {
                    AUIElement child = _children[0]; 
                    child.Rect = new FloatRect(
                        default, default,
                        w, Rect.Height
                    );
                }
                else
                {
                    int optimalWidth = (int)((Rect.Width - Host.Style.AxisBoxSpace()*(_children.Count-1)) / _children.Count);
                    List<int> doNotFit = new();

                    for (var i = 0; i < _children.ToArray().Length; i++)
                    {
                        AUIElement child = _children.ToArray()[i];
                    
                        if (child.MinimalSize.X > optimalWidth)
                        {
                            w = (int)(Rect.Width - child.MinimalSize.X);
                            doNotFit.Add(i);
                            child.Rect = new FloatRect(default, new Vector2f(child.MinimalSize.X, Rect.Height));

                            if (doNotFit.Count == _children.Count) break;
                        
                            optimalWidth = w / (_children.Count - doNotFit.Count);
                            i = -1; //restarting
                        }
                        else
                        {
                            child.Rect = new FloatRect(default, new Vector2f(optimalWidth, Rect.Height));
                        }
                    }   
                }
            }
            else
            {
                foreach (AUIElement child in _children)
                {
                    child.Rect = new FloatRect(
                        default, default,
                        child.MinimalSize.X, Rect.Height
                    );
                }
            }

            foreach (AUIElement child in _children)
            {
                child.Rect = new FloatRect(position, child.Rect.Size);
                position.X += child.Rect.Size.X + Host.Style.AxisBoxSpace();
            }
        }
        else
        {
            //todo fitrect
            foreach (AUIElement child in _children.ToArray())
            {
                child.Rect = new FloatRect(
                    position.X, position.Y,
                    Rect.Width, child.MinimalSize.Y
                );
                
                position.Y += child.MinimalSize.Y + Host.Style.AxisBoxSpace();
            }
        }
    }
    
    protected override void UpdateMinimalSize()
    {
        MinimalSize = CalculateSize(Host.Style, _axis, _children);
        UpdateLayout();
    }

    public override void Draw(RenderTarget target)
    {
        foreach (AUIElement child in _children)
        {
            Host.DrawStack.Push(child.Draw);
        }
    }
}