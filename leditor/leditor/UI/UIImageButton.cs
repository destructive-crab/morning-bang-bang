using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIImageButton : AUIElement
{
    private readonly RectangleShape _shape = new();
    private readonly Sprite _sprite;

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
        _sprite.Position = Rect.Position + style.ContentOffset;
        _shape.FillColor = style.BgColor;
    }

    private void OnHover()
        => ApplyStyle(Host.Style.HoveredButton);

    private void OnUnhover()
        => ApplyStyle(Host.Style.NormalButton);
    
    public UIImageButton(UIHost host, Texture texture, Vector2f scale = default, Action? action = null) : 
        base(host, Utils.ScaleVec(Utils.VecU2F(texture.Size), scale) + host.Style.ButtonSpace)
    {
        _sprite = new Sprite(texture)
        {
            Scale = scale
        };
        _area.OnClick = action;
        _area.OnHover = OnHover;
        _area.OnUnhover = OnUnhover;

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
    }

    public override void Draw(RenderTarget target)
    {
        target.Draw(_shape);
        target.Draw(_sprite);
    }
}