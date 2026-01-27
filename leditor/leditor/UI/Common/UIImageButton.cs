using System;
using leditor.root;
using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIImageButton : AUIElement
{
    private readonly RectangleShape outlineShape = new();
    private readonly Sprite sprite;

    private readonly ClickArea area = new(default);

    public Action? Action;

    private ButtonStateStyle appliedStyle;

    public UIImageButton(UIHost host, Texture texture, Rect rect, Vector2f scale = default, Action? action = null) : 
        base(host, Utils.ScaleVec(Utils.VecI2F(rect.ToIntRect().Size), scale) + host.Style.ButtonSpace())
    {
        sprite = new Sprite(texture)
        {
            Scale = scale,
        };
        
        if (rect != UTLS.FULL)
        {
            sprite.TextureRect = rect.ToIntRect();
        }
        
        area.OnRightMouseButtonClick = OnPress;
        area.OnRightMouseButtonReleased = OnReleased;
        area.OnHover = OnHover;
        area.OnUnhover = OnUnhover;

        Action = action;
        ApplyStyle(host.Style.NormalButton());
    }

    private void ApplyStyle(ButtonStateStyle style)
    {
        sprite.Position         = Rect.Position + new Vector2f(style.Outline, style.Outline);
        
        outlineShape.FillColor  = style.OutlineColor;
        outlineShape.Position   = sprite.Position - new Vector2f(style.Outline, style.Outline);

        outlineShape.Size       = sprite.GetGlobalBounds().Size + 2 * new Vector2f(style.Outline, style.Outline);
        outlineShape.FillColor  = style.OutlineColor;
        
        appliedStyle = style;
    }

    private void OnHover()           => ApplyStyle(Host.Style.HoveredButton());
    private void OnUnhover()         => ApplyStyle(Host.Style.NormalButton());
    protected virtual void OnPress() => ApplyStyle(Host.Style.PressedButton());
    protected virtual void OnReleased()
    {
        ApplyStyle(Host.Style.HoveredButton());
        Action?.Invoke();
    }
    
    public override void ProcessClicks()
    {
        Host.Areas.Process(area);
    }

    protected override void UpdateLayoutIm()
    {
        area.Rect = Rect;
        ApplyStyle(appliedStyle);
    }

    public override void Draw(RenderTarget target)
    {
        target.Draw(outlineShape);
        target.Draw(sprite);
    }
}