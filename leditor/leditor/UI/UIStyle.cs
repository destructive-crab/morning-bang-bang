using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIStyle()
{
    private static Font PrepareFont()
    {
        var font = new Font("./Autistic.ttf");
        
        font.SetSmooth(false);

        return font;
    }
    
    // Text
    public readonly Font Font = PrepareFont();
    public readonly uint FontSize = 16;
    
    // Label
    public readonly Color LabelColor = Color.White;
    
    // AxisBox
    public readonly int AxisBoxSpace = 2;
    
    // SplitBox
    public readonly int SplitSeparatorThickness = 5;
    public readonly Color SplitSeparatorColor = Color.Cyan;
    
    // Button
    public readonly Vector2f ButtonSpace = new(6, 3);
    public readonly ButtonStateStyle NormalButton = new()
    {
        TextOffset = new Vector2f(3, 0),
        TextColor = Color.Black,
        BgColor = Color.White
    };
    public readonly ButtonStateStyle HoveredButton = new()
    {
        TextOffset = new Vector2f(3, 0),
        TextColor = Color.White,
        BgColor = Color.Black
    };
    
    //ScrollBox
    public readonly float ScrollerThickness = 12;
    public readonly Color ScrollerColor = Color.Cyan;
}