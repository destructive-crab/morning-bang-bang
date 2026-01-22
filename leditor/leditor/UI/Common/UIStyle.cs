using leditor.root;
using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIStyle
{
    public virtual Color FirstBackgroundColor()  => new(0x282c34FF);
    public virtual Color SecondBackgroundColor() => new(0x15171bFF);
    
    public virtual int   BaseOutline()           => 5;
    public virtual Color OutlineColor()          => new(0x4f545eFF);

    private static Font PrepareFont() => new(EditorAssets.LoadFont("Main.ttf"));

    // Text
    public virtual Font Font()     => PrepareFont();
    public virtual uint FontSize() => 16;
    
    // Rect
    public virtual Color RectDefault() => FirstBackgroundColor();
    
    //Label
    public virtual Color LabelColor() => new(0xc0c0c0FF);
    
    //AxisBox
    public virtual int AxisBoxSpace() => 8;
    
    //SplitBox
    public virtual int SplitSeparatorThickness() => 2;
    public virtual Color SplitSeparatorColor() => new(0x181a1fFF);
    
    //Entry
    public virtual Color EntryBackgroundColor() => SecondBackgroundColor();
    public virtual Color CursorColor() => new(0x6086d1FF);
    public virtual int CursorWidth() => 4;
    public virtual int BoxSizeX() => 4;
    public virtual int BoxSizeY() => 12;
    
    //Button
    public virtual Vector2f ButtonSpace() => new(8, 8);
    public virtual Color ButtonTop() => new(0x5d84d1FF);
    public virtual Color ButtonBottom() => new(0x3c424fFF);
    
    public virtual ButtonStateStyle NormalButton() => new()
    {
        ContentOffset = new Vector2f(0, 4),
        
        TextColor     = LabelColor(),
        
        TopColor      = ButtonTop(),
        BottomColor   = ButtonBottom(),
        
        Outline       = 2,
        OutlineColor  = Color.Black,
        
        BottomHeight  = 4
    };

    public virtual ButtonStateStyle HoveredButton() => new()
    {
        ContentOffset = new Vector2f(0, 4),
        
        TextColor     = LabelColor(),
        
        TopColor      = ButtonTop(),
        BottomColor   = ButtonBottom(),
        
        OutlineColor  = new Color(0xfdea70FF),
        Outline       = 2,
        
        BottomHeight  = 2

    };

    public virtual ButtonStateStyle PressedButton() => new()
    {
        ContentOffset = new Vector2f(0, 4),
        
        TextColor     = LabelColor(),
        
        TopColor      = ButtonTop(),
        BottomColor   = ButtonBottom(),
        
        BottomHeight  = 2,
        
        Outline       = 1,
        OutlineColor  = Color.Black,
    };

    //ScrollBox
    public virtual float ScrollerThickness() => 16;
    public virtual Color ScrollerColor() => ButtonTop();
    public virtual Color ScrollerPressedColor() => ButtonBottom();
    
    //TabBox
    public virtual int TabLineHeight() => 30;
}