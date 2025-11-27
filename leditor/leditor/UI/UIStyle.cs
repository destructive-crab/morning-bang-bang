using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIStyle()
{
    private static Font PrepareFont()
    {
        Font font 
        = new("C:\\Users\\destructive_crab\\dev\\band-bang\\leditor\\leditor\\assets\\Roboto.ttf");

        return font;
    }
    
    // Text
    public readonly Font Font = PrepareFont();
    public readonly uint FontSize = 12;
    
    // Label
    public readonly Color LabelColor = new(0xdee2e6FF);
    
    // AxisBox
    public readonly int AxisBoxSpace = 3;
    
    // SplitBox
    public readonly int SplitSeparatorThickness = 4;
    public readonly Color SplitSeparatorColor = new(0x212529FF);
    
    // Button
    public readonly Vector2f ButtonSpace = new(8, 8);
    public readonly ButtonStateStyle NormalButton = new()
    {
        ContentOffset = new Vector2f(4, 4),
        TextColor = new Color(0xdee2e6FF),
        BgColor = new Color(0x495057FF)
    };
    public readonly ButtonStateStyle HoveredButton = new()
    {
        ContentOffset = new Vector2f(4, 4),
        TextColor = new Color(0xe9ecefFF),
        BgColor = new Color(0x6c757dFF)
    };
    
    //ScrollBox
    public readonly float ScrollerThickness = 8;
    public readonly Color ScrollerColor = new(0x6c757dFF);
}