using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIFabric(UIHost host)
{
    private UIHost _host = host;

    public Text MakeText(string str, int fontSize = -1)
    {
        if(fontSize == -1) fontSize = (int)_host.Style.FontSize;
        
        var text = new Text(str, _host.Style.Font, (uint)fontSize)
        {
            Style = Text.Styles.Bold,
            FillColor = _host.Style.LabelColor
        };

        return text;
    }

    public RectangleShape MakeRect(Color color, Vector2f size = default)
        => new(size)
        {
            FillColor = color
        };

    public Vector2f MakeTextOut(string str, out Text text, int fontSize = -1)
    {
        text = MakeText(str, fontSize);
        return Utils.TextSize(text);
    }
}