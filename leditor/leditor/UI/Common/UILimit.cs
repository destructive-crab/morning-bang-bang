using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UILimit(UIHost host, Vector2f minimalSize) : AUIElement(host, minimalSize)
{
    protected override void UpdateLayout() {}

    public override void Draw(RenderTarget target) {}
}