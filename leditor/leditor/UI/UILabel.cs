using System.Numerics;
using Raylib_cs;

namespace leditor.UI;

public class UILabel(UIHost host, string text = "") : AUIElement(host, GetMinimalSize(host.Style, text))
{
    private static Vector2 GetMinimalSize(UIStyle style, string text)
        => Raylib.MeasureTextEx(style.Font, text, style.FontSize, 1);
    
    private string _text = text;

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            MinimalSize = GetMinimalSize(Host.Style, _text);
        }
    }

    public override void UpdateLayout() {}

    public override void Draw()
    {
        var style = Host.Style;
        Raylib.DrawTextEx(
            style.Font, Text, Rect.Position, 
            style.FontSize, 1, style.LabelColor
        );
    }
}