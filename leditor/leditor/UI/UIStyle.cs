using Raylib_cs;

namespace leditor.UI;

public class UIStyle(Color labelColor, Font font, float fontSize, float axisBoxSpace)
{
    public Color LabelColor = labelColor;
    public Font Font = font;
    public float FontSize = fontSize;
    public float AxisBoxSpace = axisBoxSpace;
}