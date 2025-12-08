using leditor.root;
using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public struct ButtonStateStyle
{
    public Vector2f ContentOffset;
    public Color TextColor;
    public Color TopColor;
    public Color BottomColor;
    public Color OutlineColor;
    public float Outline;
    public float BottomHeight;
}

public class UIButton : AUIElement
{
    private readonly RectangleShape shapeTop     = new();
    private readonly RectangleShape shapeBottom  = new();
    private readonly RectangleShape shapeOutline = new();
    
    private readonly Text _textObj;
    
    public string Text
    {
        get => _textObj.DisplayedString;
        set
        {
            _textObj.DisplayedString = value;
            
            MinimalSize = Utils.TextSize(_textObj) + Host.Style.ButtonSpace;
        }
    }

    private readonly ClickArea _area = new(default);

    public Action? Action;

    private Vector2f _styleTextOffset;

    protected ButtonStateStyle appliedStyle;
    
    public UIButton(UIHost host, string text, Action? action = null) : 
        base(host, host.Fabric.MakeTextOut(text, out var textObj) + host.Style.ButtonSpace + new Vector2f(0, host.Style.NormalButton.BottomHeight))
    {
        _textObj = textObj;

        Action = action;
        
        _area.OnRightMouseButtonClick = OnPress;
        _area.OnRightMouseButtonReleased = OnReleased;
        _area.OnHover = OnHover;
        _area.OnUnhover = OnUnhover;
        
        ApplyStyle(host.Style.NormalButton);
    }

    protected virtual void ApplyStyle(ButtonStateStyle style)
    {
        int bottomY      = (int)(Rect.Position.Y + MinimalSize.Y);
        
        int topHeight    = (int)(_textObj.CharacterSize + Host.Style.ButtonSpace.Y);
        int bottomHeight = (int)(style.BottomHeight);

        int x            = (int)(Rect.Size.X - style.Outline - 2);
        
        shapeBottom.Position   = new Vector2f(Rect.Position.X, bottomY-bottomHeight);
        shapeBottom.Position   += new Vector2f(style.Outline, 0);
        shapeBottom.Size       = new Vector2f(x, bottomHeight);
        shapeBottom.FillColor  = style.BottomColor;

        shapeTop.Position      = new Vector2f(shapeBottom.Position.X, shapeBottom.Position.Y-topHeight);
        shapeTop.Size          = new Vector2f(x, topHeight);
        shapeTop.FillColor     = style.TopColor;

        shapeOutline.Position  = shapeTop.Position - new Vector2f(style.Outline, style.Outline);
        shapeOutline.Size      = Rect.Size + new Vector2f(style.Outline, style.Outline);
        shapeOutline.Size      = new Vector2f(Rect.Size.X, shapeTop.Size.Y + shapeBottom.Size.Y + style.Outline);
        shapeOutline.FillColor = style.OutlineColor;
        
        _styleTextOffset       = Host.Style.ButtonSpace / 2;
        _textObj.Position      = shapeTop.Position + _styleTextOffset;

        appliedStyle = style;
    }

    protected virtual void OnHover()     => ApplyStyle(Host.Style.HoveredButton);
    protected virtual void OnUnhover()   => ApplyStyle(Host.Style.NormalButton);
    protected virtual void OnPress()     => ApplyStyle(Host.Style.PressedButton);
    protected virtual void OnReleased()  => Action?.Invoke();

    public override void ProcessClicks() => Host.Areas.Process(_area);
    public override void UpdateLayout()
    {
        _area.Rect = Rect;
     
        ApplyStyle(appliedStyle);
        
 //       shapeTop.Position = Re
   //     _textObj.Position = Rect.Position + _styleTextOffset;
        //shapeTop.Size = Rect.Size;
        //shapeTop.Position = Rect.Position;
        //shapeBottom.Position = Rect.Position;
        //shapeOutline.Position = Rect.Position - new Vector2f(appliedStyle.Outline, appliedStyle.Outline);
    }
    public override void Draw(RenderTarget target)
    {
        target.Draw(shapeOutline);
        target.Draw(shapeBottom);
        target.Draw(shapeTop);
        target.Draw(_textObj);
    }
}