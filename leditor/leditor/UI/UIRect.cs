using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIRect : AUIElement
{
    private Vector2f outline;
    private readonly RectangleShape shape;
    private readonly RectangleShape outlineShape;

    public UIRect(UIHost host, Color? color = null, Vector2f outline = default) : base(host, default)
    {
        shape = new RectangleShape
        {
            FillColor = color ?? host.Style.RectDefault
        };
        this.outline = outline;
        this.outlineShape = new RectangleShape();
        outlineShape.FillColor = host.Style.RectDefault;
    }

    public Color Color
    {
        get => shape.FillColor;
        set => shape.FillColor = value;
    }

    public override void UpdateLayout()
    {
        outlineShape.Position = Rect.Position;
        outlineShape.Size = Rect.Size + 2 * outline;
        
        shape.Position = Rect.Position + outline;
        shape.Size = Rect.Size;
    }

    public override void Draw(RenderTarget target)
    {
        target.Draw(outlineShape);
        target.Draw(shape);
    }
}