using Raylib_cs;

namespace leditor.UI;

public class UIStyle(Color labelColor, Font font, int fontSize, int axisBoxSpace)
{
    public readonly Color LabelColor = labelColor;
    public readonly Font Font = font;
    public readonly int FontSize = fontSize;
    public readonly int AxisBoxSpace = axisBoxSpace;
    public int SplitSeparatorThickness = 5;
    public Color SplitSeparatorColor = Color.Beige;
}