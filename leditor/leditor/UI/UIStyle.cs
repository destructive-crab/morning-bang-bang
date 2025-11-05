using Raylib_cs;

namespace leditor.UI;

public class UIStyle
{
    public Color LabelColor;
    public Font Font;
    public float FontSize;

    public UIStyle(Color labelColor, Font font, float fontSize)
    {
        LabelColor = labelColor;
        Font = font;
        FontSize = fontSize;
    }
}