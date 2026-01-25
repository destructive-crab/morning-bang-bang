using System;
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
    
    private readonly Text textObj;
    
    public string Text
    {
        get => textObj.DisplayedString;
        set
        {
            textObj.DisplayedString = value;
        }
    }

    private readonly ClickArea area = new(default);

    public Action? Action;

    private Vector2f _styleTextOffset;

    protected ButtonStateStyle appliedStyle;
    
    public UIButton(UIHost host, string text, Action? action = null) : 
        base(host, 
/*minimal size*/ host.Fabric.MakeTextOut(text, out Text textObj) + host.Style.ButtonSpace() + new Vector2f(8, host.Style.NormalButton().BottomHeight + host.Style.NormalButton().Outline))
    {
        this.textObj = textObj;

        Action = action;
        BuildClickArea();
        ApplyStyle(host.Style.NormalButton());
    }
    
    public UIButton(UIHost host, string text, Vector2f minimalSize, Action? action = null) : 
        base(host, minimalSize)
    {
        host.Fabric.MakeTextOut(text, out Text textObj);
        this.textObj = textObj;

        Action = action;
        BuildClickArea();
        ApplyStyle(host.Style.NormalButton());
    }

    private void BuildClickArea()
    {
        area.OnRightMouseButtonClick    = OnPress;
        area.OnRightMouseButtonReleased = OnReleased;
        area.OnHover                    = OnHover;
        area.OnUnhover                  = OnUnhover;
    }

    protected virtual void ApplyStyle(ButtonStateStyle style)
    {
        int bottomY      = (int)(Rect.Position.Y + MinimalSize.Y);
        
        int topHeight    = (int)(textObj.CharacterSize + Host.Style.ButtonSpace().Y);
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
        
        _styleTextOffset       = Host.Style.ButtonSpace() / 2;
        textObj.Position       = shapeTop.Position + new Vector2f(shapeTop.Size.X / 2f - textObj.GetGlobalBounds().Size.X / 2f, 0) + style.ContentOffset;
        textObj.FillColor      = style.TextColor;

        appliedStyle = style;
    }

    protected virtual void OnHover()   => ApplyStyle(Host.Style.HoveredButton());
    protected virtual void OnUnhover() => ApplyStyle(Host.Style.NormalButton());
    protected virtual void OnPress()   => ApplyStyle(Host.Style.PressedButton());

    protected virtual void OnReleased()
    {
        ApplyStyle(Host.Style.HoveredButton());
        Action?.Invoke();
    }

    public override void ProcessClicks() => Host.Areas.Process(area);

    protected override void UpdateLayout()
    {
        area.Rect = Rect;
     
        ApplyStyle(appliedStyle);
    }
    public override void Draw(RenderTarget target)
    {
        target.Draw(shapeOutline);
        target.Draw(shapeBottom);
        target.Draw(shapeTop);
        target.Draw(textObj);
    }
}