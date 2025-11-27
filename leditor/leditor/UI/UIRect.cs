using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIRect(UIHost host, Color? color = null) : AUIElement(host, new Vector2f())
{
    private readonly RectangleShape _shape = new()
    {
        FillColor = color ?? Color.White
    };

    public Color Color
    {
        get => _shape.FillColor;
        set => _shape.FillColor = value;
    }

    public override void UpdateLayout()
    {
        _shape.Position = Rect.Position;
        _shape.Size = Rect.Size;
    }

    public override void Draw(RenderTarget target)
        => target.Draw(_shape);
}