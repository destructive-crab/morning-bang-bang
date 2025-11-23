using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIClickArea(UIHost host, Vector2f minimalSize = default) : AUIElement(host, minimalSize)
{
    public ClickArea Area = new(default);

    public override void UpdateLayout()
        => Area.Rect = Rect;

    public override void ProcessClicks()
        => Host.Areas.Process(Area);

    public override void Draw(RenderTarget target) {}
}