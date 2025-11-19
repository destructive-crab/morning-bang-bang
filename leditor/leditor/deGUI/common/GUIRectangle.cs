using SFML.Graphics;

namespace deGUISpace;

public class GUIRectangle : GUIElement
{
    public Color Color = Color.White;
    public int Outline = 0;
    public Color OutlineColor = Color.Black;

    public GUIRectangle(Color color, int outline, Color outlineColor)
    {
        Outline = outline;
        Color = color;
        OutlineColor = outlineColor;
    }
    
    public GUIRectangle(Color color, int outline, Color outlineColor, RectGUIArea area)
    {
        Outline = outline;
        Color = color;
        OutlineColor = outlineColor;
        GUIArea = area;
    }
}