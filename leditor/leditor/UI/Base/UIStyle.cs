using leditor.root;
using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIStyle()
{
    private static Font PrepareFont()
    {
        Font font = new(EditorAssets.LoadFont("Main.ttf"));

        return font;
    }
    
    // Text
    public readonly Font Font = PrepareFont();
    public readonly uint FontSize = 14;
    
    // Label
    public readonly Color LabelColor = new(0x000000FF);
    
    // AxisBox
    public readonly int AxisBoxSpace = 5;
    
    // SplitBox
    public readonly int SplitSeparatorThickness = 2;
    public readonly Color SplitSeparatorColor = new(0x000000FF);
    
    // UI Entry
    public readonly Color EntryBackgroundColor = new(0xFFFFFFFF);
    public readonly int EntryOutline = 2;
    public readonly Color EntryOutlineColor = new(0x000000FF);
    public readonly Color EntrySelectedOutlineColor = new(0xF00000FF);
    public readonly Color CursorColor = new(0x000000FF);
    public readonly int CursorWidth = 4;
    public readonly int BoxSizeX = 4;
    public readonly int BoxSizeY = 10;
    
    // Button
    public readonly Vector2f ButtonSpace = new(8, 8);
    public readonly ButtonStateStyle NormalButton = new()
    {
        ContentOffset = new Vector2f(4, 4),
        
        TextColor =     new Color(0x000000FF),
        
        TopColor =      new Color(0xFFFFFFFF),
        BottomColor =   new Color(0xF00000FF),
        
        Outline = 2,
        OutlineColor = new Color(0x000000FF),
        
        BottomHeight = 4
    };
    
    public readonly ButtonStateStyle HoveredButton = new()
    {
        ContentOffset = new Vector2f(4, 4),
        
        TextColor =     new Color(0xd8dee9FF),
        
        TopColor =      new Color(0x9cac3fFF),
        BottomColor =   new Color(0x42652bFF),
        
        OutlineColor =  new Color(0xeadb94FF),
        Outline = 2,
        
        BottomHeight = 5
    };

    public readonly Color RectDefault = new(0xFFFFFFFF);
    public readonly ButtonStateStyle PressedButton = new()
    {
        ContentOffset = new Vector2f(4, 4),
        TextColor = new Color(0xd8dee9FF),
        TopColor =      new Color(0x9cac3fFF),
        BottomColor =   new Color(0x42652bFF),
        
        Outline = 2,
        BottomHeight = 2,
        OutlineColor =  new Color(0x42652bFF),
    };
    
    //ScrollBox
    public readonly float ScrollerThickness = 16;
    public readonly Color ScrollerColor = new(0x000000FF);
    public readonly Color ScrollerPressedColor = new(0xF00000FF);
}