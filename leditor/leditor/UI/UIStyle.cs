using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIStyle()
{
    private static Font PrepareFont()
    {
        Font font 
        = new("C:\\Users\\destructive_crab\\dev\\band-bang\\leditor\\leditor\\assets\\Main.ttf");

        return font;
    }
    
    // Text
    public readonly Font Font = PrepareFont();
    public readonly uint FontSize = 14;
    
    // Label
    public readonly Color LabelColor = new(0xdee2e6FF);
    
    // AxisBox
    public readonly int AxisBoxSpace = 5;
    
    // SplitBox
    public readonly int SplitSeparatorThickness = 2;
    public readonly Color SplitSeparatorColor = new(0x4c566aFF);
    
    // UI Entry
    public readonly Color EntryBackgroundColor = new(0x4c566aFF);
    public readonly Color CursorColor = new(0xd8dee9FF);
    public readonly int CursorWidth = 4;
    public readonly int BoxSizeX = 4;
    public readonly int BoxSizeY = 12;
    
    // Button
    public readonly Vector2f ButtonSpace = new(8, 8);
    public readonly ButtonStateStyle NormalButton = new()
    {
        ContentOffset = new Vector2f(4, 4),
        TextColor = new Color(0xd8dee9FF),
        BgColor = new Color(0x434c5eFF)
    };
    
    public readonly ButtonStateStyle HoveredButton = new()
    {
        ContentOffset = new Vector2f(4, 4),
        TextColor = new Color(0xe9ecefFF),
        BgColor = new Color(0x4c566aFF)
    };
    
    public readonly ButtonStateStyle PressedButton = new()
    {
        ContentOffset = new Vector2f(4, 4),
        TextColor = new Color(0xd8dee9FF),
        BgColor = new Color(0x2c3342FF)
    };
    
    //ScrollBox
    public readonly float ScrollerThickness = 16;
    public readonly Color ScrollerColor = new(0x4c566aFF);
    public readonly Color ScrollerPressedColor = new(0x4c566a90);
}