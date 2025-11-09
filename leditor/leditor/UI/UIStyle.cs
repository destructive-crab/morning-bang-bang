using System.Numerics;
using Raylib_cs;

namespace leditor.UI;

public class UIStyle()
{
    // Text
    public readonly Font Font = Raylib.GetFontDefault();
    public readonly int FontSize = 24;
    public readonly int FontSpacing = 1;
    
    // Label
    public readonly Color LabelColor = Color.LightGray;
    
    // AxisBox
    public readonly int AxisBoxSpace = 2;
    
    // SplitBox
    public readonly int SplitSeparatorThickness = 5;
    public readonly Color SplitSeparatorColor = Color.Beige;
    
    // Button
    public readonly Vector2 ButtonSpace = Vector2.One * 6;
    public readonly ButtonStateStyle NormalButton = new()
    {
        TextOffset = Vector2.One * 3,
        TextColor = Color.LightGray,
        BgColor = Color.DarkGray
    };
    public readonly ButtonStateStyle HoveredButton = new()
    {
        TextOffset = Vector2.One * 3,
        TextColor = Color.White,
        BgColor = Color.Gray
    };
}