using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UILabel : AUIElement
{
    
    public UILabel(UIHost host, string text = "") : 
        base(host, host.Fabric.MakeTextOut(text, out var textObj))
    {
        _textObj = textObj;
    }

    private Text _textObj;
    public string Text
    {
        get => _textObj.DisplayedString;
        set
        {
            _textObj.DisplayedString = value;
            MinimalSize = Utils.TextSize(_textObj);
        }
    }

    public override void UpdateLayout()
    {
        _textObj.Position = Rect.Position;
    }

    public override void Draw(RenderTarget target)
    {
        target.Draw(_textObj);
    }
}