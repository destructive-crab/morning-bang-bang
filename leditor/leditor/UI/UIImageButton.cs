using leditor.root;
using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIImageButton : AUIElement
{
    private readonly RectangleShape _shape = new();
    private readonly Sprite _sprite;

    private readonly ClickArea _area = new(default);

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
    
    public Action? Action
    {
        set => _area.OnClick = value;
        get => _area.OnClick;
    }

    private Vector2f _styleTextOffset;
    private void ApplyStyle(ButtonStateStyle style)
    {
        _styleTextOffset = style.ContentOffset;
        _sprite.Position = Rect.Position + style.ContentOffset;
        _shape.FillColor = style.BgColor;
        
        _textObj.FillColor = style.TextColor;
        _textObj.Position = Rect.Position + style.ContentOffset;
        _shape.FillColor = style.BgColor;
    }

    private void OnHover()
        => ApplyStyle(Host.Style.HoveredButton);

    private void OnUnhover()
        => ApplyStyle(Host.Style.NormalButton);
    
    public UIImageButton(UIHost host, Texture texture, Rect rect, string text = "", Vector2f scale = default, Action? action = null) : 
        base(host, Utils.ScaleVec(Utils.VecI2F(rect.ToIntRect().Size), scale) + host.Style.ButtonSpace)
    {
        _sprite = new Sprite(texture)
        {
            Scale = scale,
        };
        if (rect != UTLS.FULL)
        {
            _sprite.TextureRect = rect.ToIntRect();
        }
        _area.OnClick = action;
        _area.OnHover = OnHover;
        _area.OnUnhover = OnUnhover;
        host.Fabric.MakeTextOut(text, out Text textObj);
        _textObj = textObj;

        ApplyStyle(host.Style.NormalButton);
    }

    public override void ProcessClicks()
    {
        Host.Areas.Process(_area);
    }

    public override void UpdateLayout()
    {
        _area.Rect = Rect;
        _sprite.Position = Rect.Position + _styleTextOffset;
        _shape.Size = Rect.Size;
        _shape.Position = Rect.Position;
        _textObj.Position = Rect.Position + _styleTextOffset;
    }

    public override void Draw(RenderTarget target)
    {
        target.Draw(_shape);
        target.Draw(_sprite);
        target.Draw(_textObj);
    }
}