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

    private readonly List<AUIElement> children;
    private readonly UIAxis _axis;

    private readonly bool FitRect;
    private readonly List<AUIElement> useMinimalRectWhenFitFor = new();

    public AxisBox(UIHost host, UIAxis axis, params AUIElement[] children) : this(host, axis, false, children) { }
    public AxisBox(UIHost host, UIAxis axis, bool fitRect = false, params AUIElement[] children) : base(host, CalculateSize(host.Style, axis, children))
    {
        FitRect = fitRect;
        foreach (var child in children)
        {
            child.Parent = this;
        }

        _axis = axis;
        this.children = new List<AUIElement>(children);
    }

    public override IEnumerable<AUIElement> GetChildren()
        => children;

    public override void RemoveChild(AUIElement child)
    {
        if (useMinimalRectWhenFitFor.Contains(child)) useMinimalRectWhenFitFor.Remove(child);
        
        child.Parent = null;
        children.Remove(child);
        UpdateMinimalSize();
    }

    public AUIElement AddChild(AUIElement child)
    {
        child.Parent = this;
        children.Add(child);
        UpdateMinimalSize();
        return child;
    }

    public void UseMinimalSizeFor(AUIElement child)
    {
        if (!children.Contains(child) || useMinimalRectWhenFitFor.Contains(child)) return;
        
        useMinimalRectWhenFitFor.Add(child);
        UpdateLayout();
    }
    
    public override void UpdateLayout()
    {
        Console.WriteLine($"Start Updating Layout For {children.Count}; fit: {FitRect}; Rect: {Rect.Position} {Rect.Size}");
        if (children.Count == 1)
        {
            AUIElement child = children[0];
            child.Rect = Rect;

            return;
        }
        
        if (_axis == UIAxis.Horizontal)
        {
            if (FitRect)
            {
                MakeChildrenFitRect((int)Rect.Width, (int)Rect.Height, 
                    (c) => (int)c.MinimalSize.X, 
                    (c, v) => c.Rect = new FloatRect(c.Rect.Position, new Vector2f(v, Rect.Height)));
            }
            else
            {
                foreach (AUIElement child in children)
                {
                    child.Rect = new FloatRect(
                        default, default,
                        child.MinimalSize.X, Rect.Height
                    );
                }
            }
            
            UpdatePositions((c) => new(c.Rect.Size.X, 0), () => new(Host.Style.AxisBoxSpace(), 0));
        }
        else
        {
            if (FitRect)
            {
                MakeChildrenFitRect((int)Rect.Height, (int)Rect.Width, 
                    (c) => (int)c.MinimalSize.Y, 
                    (c, v) => c.Rect = new FloatRect(c.Rect.Position, new Vector2f(Rect.Width, v)));
            }
            else
            {
                foreach (AUIElement child in children)
                {
                    Console.WriteLine($"Set Size For {child.GetType()} Y: {child.MinimalSize.Y}");
                    child.Rect = new FloatRect(
                        child.Rect.Position.X, child.Rect.Position.Y,
                        Rect.Width, child.MinimalSize.Y
                    );
                }
            }
            
            UpdatePositions((c) => new(0, c.Rect.Size.Y), () => new(0, Host.Style.AxisBoxSpace()));
        }
        Console.WriteLine($"End Updating Layout For {children.Count}; fit: {FitRect}");
    }

    private void MakeChildrenFitRect(
        int originalToFit,
        int staticRectValue,
        Func<AUIElement, int> getMinimal, 
        Action<AUIElement, int> setSize)
    {
        List<AUIElement> doNotFit = new();
        
        int doNotFitValue = 0;
        foreach (AUIElement element in useMinimalRectWhenFitFor)
        {
            doNotFitValue += getMinimal(element);
            doNotFit.Add(element);
        }

        int axisBoxSpacesValue = Host.Style.AxisBoxSpace()*(children.Count-1);
        int valueToFit = originalToFit - axisBoxSpacesValue - doNotFitValue;
        int optimalValue = valueToFit / (children.Count - doNotFit.Count);

        for (var i = 0; i < children.ToArray().Length; i++)
        {
            AUIElement child = children.ToArray()[i];

            if (doNotFit.Contains(child))
            {
                setSize(child, (int)getMinimal(child));   
                continue;
            }
                    
            if (getMinimal(child) > optimalValue)
            {
                valueToFit -= (int)getMinimal(child);
                doNotFit.Add(child);
                
                setSize(child, (int)getMinimal(child));
                
                if (doNotFit.Count == children.Count) break;
                        
                optimalValue = valueToFit / (children.Count - doNotFit.Count);
                i = -1; //restarting
            }
            else
            {
                setSize(child, optimalValue);
            }
        }   
    }

    private void UpdatePositions(Func<AUIElement, Vector2f> getPositionStep, Func<Vector2f> getAxisSpace)
    {
        Vector2f position = Rect.Position;

        foreach (AUIElement child in children)
        {
            child.Rect = new FloatRect(position, child.Rect.Size);
            Console.WriteLine($"Set Position For {child.GetType()} {position}");
            position += getPositionStep(child) + getAxisSpace();
        }
    }
    
    protected override void UpdateMinimalSize()
    {
        MinimalSize = CalculateSize(Host.Style, _axis, children);
        UpdateLayout();
    }

    public override void Draw(RenderTarget target)
    {
        foreach (AUIElement child in children)
        {
            Host.DrawStack.Push(child.Draw);
        }
    }
}