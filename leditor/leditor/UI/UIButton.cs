using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public struct ButtonStateStyle
{
    public Vector2f ContentOffset;
    public Color TextColor;
    public Color BgColor;
}

public class UIButton : AUIElement
{
    private readonly RectangleShape _shape = new();
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

    public Action? Action
    {
        set => _area.OnClick = value;
        get => _area.OnClick;
    }

    private Vector2f _styleTextOffset;
    private void ApplyStyle(ButtonStateStyle style)
    {
        _styleTextOffset = style.ContentOffset;
        _textObj.FillColor = style.TextColor;
        _textObj.Position = Rect.Position + style.ContentOffset;
        _shape.FillColor = style.BgColor;
    }

    private void OnHover()
        => ApplyStyle(Host.Style.HoveredButton);

    private void OnUnhover()
        => ApplyStyle(Host.Style.NormalButton);
    
    public UIButton(UIHost host, string text, Action? action = null) : 
        base(host, host.Fabric.MakeTextOut(text, out var textObj) + host.Style.ButtonSpace)
    {
        _textObj = textObj;
        _area.OnClick = action;
        _area.OnHover = OnHover;
        _area.OnUnhover = OnUnhover;

        ApplyStyle(host.Style.NormalButton);
    }
    
    public override void ProcessClicks()
        => Host.Areas.Process(_area);

    public override void UpdateLayout()
    {
        _area.Rect = Rect;
        _textObj.Position = Rect.Position + _styleTextOffset;
        _shape.Size = Rect.Size;
        _shape.Position = Rect.Position;
    }

    public override void Draw(RenderTarget target)
    {
        target.Draw(_shape);
        target.Draw(_textObj);
    }
}