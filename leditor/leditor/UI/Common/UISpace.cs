using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public sealed class UISpace(UIHost host, Vector2f minimalSize) : AUIElement(host, minimalSize)
{
    protected override void UpdateLayoutIm()
    {
    }

    public override void Draw(RenderTarget target)
    {
        
    }
}