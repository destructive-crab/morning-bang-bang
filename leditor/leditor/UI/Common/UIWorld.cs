using leditor.root;
using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIWorld : AUIElement
{
    public bool IsHovered;
    private ClickArea clickArea = new(default);

    public UIWorld(UIHost host) : base(host, new Vector2f())
    {
        clickArea.OnHover += () => IsHovered = true;
        clickArea.OnUnhover += () => IsHovered = false;
    }

    protected override void UpdateLayout()
    {
        clickArea.Rect = Rect;
    }

    public override void Draw(RenderTarget target)
    {
    }
    public override void ProcessClicks()
        => Host.Areas.Process(clickArea);
}