using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UILabel : AUIElement
{
    private static Vector2f MakeText(UIStyle style, string text, out Text textObj)
    {
        textObj = new Text(text, style.Font);
        textObj.CharacterSize = style.FontSize;
        textObj.Style = SFML.Graphics.Text.Styles.Bold;
        
        return textObj.GetLocalBounds().Size;
    }
    
    public UILabel(UIHost host, string text = "") : 
        base(host, MakeText(host.Style, text, out var textObj))
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
            MinimalSize = _textObj.GetLocalBounds().Size;
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