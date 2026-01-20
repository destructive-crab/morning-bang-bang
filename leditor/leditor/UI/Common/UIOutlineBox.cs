using leditor.UI;
using SFML.Graphics;
using SFML.System;

namespace deadlauncher.Other.UI;

public sealed class UIOutlineBox : AUIBox
{
    public AUIElement? Child { get; private set; }

    private readonly RectangleShape line = new RectangleShape();

    public UIOutlineBox(UIHost host, AUIElement child) : base(host, default)
    {
        line.FillColor = Host.Style.OutlineColor();
        Child = child;
    }

    public override void Draw(RenderTarget target)
    {
        if (Child == null) return;
        
        Child.Draw(target);
        
        var outline = Host.Style.BaseOutline();

        //horizontal
        line.Position = Rect.Position - new Vector2f(outline, outline);
        line.Size     = new Vector2f(Rect.Size.X + outline * 2, outline);
       
        target.Draw(line);

        line.Position += new Vector2f(0, Rect.Size.Y + outline);
        
        target.Draw(line);
        
        //vertical
        
        line.Position = Rect.Position - new Vector2f(outline, outline);
        line.Size     = new Vector2f(outline, Rect.Size.Y + outline * 2);

        target.Draw(line);

        line.Position += new Vector2f(Rect.Size.X + outline, 0);
        
        target.Draw(line);
    }

    public override IEnumerable<AUIElement> GetChildren()
    {
        return [Child];
    }

    public override void UpdateLayout()
    {
        if (Child == null) return;
        
        Child.Rect = Rect;
    }

    public override void RemoveChild(AUIElement child)
    {
        if (Child == child)
        {
            Child = null;
        }
    }

    protected override void UpdateMinimalSize() { }
}