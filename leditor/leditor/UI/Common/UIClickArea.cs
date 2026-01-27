using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIClickArea(UIHost host, ClickArea area, Vector2f minimalSize = default) : AUIElement(host, minimalSize)
{
    public ClickArea Area = area;

    protected override void UpdateLayoutIm()
        => Area.Rect = Rect;

    public override void ProcessClicks()
        => Host.Areas.Process(Area);

    public override void Draw(RenderTarget target) {}
}