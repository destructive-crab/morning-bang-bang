using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIWorld(UIHost host) : AUIElement(host, new Vector2f())
{
    private Text _text = host.Fabric.MakeText("World here!");

    public override void UpdateLayout()
    {
        _text.Position = Rect.Position + Rect.Size / 2 - _text.GetLocalBounds().Size / 2;
    }

    public override void Draw(RenderTarget target)
    {
        target.Draw(_text);
    }
}