using System.Numerics;
using Raylib_cs;

namespace leditor.UI;

public class UIRect(UIHost host, Color? color = null) : AUIElement(host, Vector2.Zero)
{
    public Color Color = color ?? Color.White;
    
    public override void UpdateLayout() { }

    public override void Draw()
        => Raylib.DrawRectangleRec(Rect, Color);
}