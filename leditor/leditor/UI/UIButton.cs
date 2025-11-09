using System.Numerics;
using Raylib_cs;

namespace leditor.UI;

public struct ButtonStateStyle
{
    public Vector2 TextOffset;
    public Color TextColor;
    public Color BgColor;
}

public class UIButton : AUIElement
{
    private static Vector2 GetMinimalSize(UIStyle style, string text)
        => Raylib.MeasureTextEx(style.Font, text, style.FontSize, style.FontSpacing) + style.ButtonSpace;

    private string _text;
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            MinimalSize = GetMinimalSize(Host.Style, value);
        }
    }

    private ClickArea _area = new(default);

    public Action? Action
    {
        set => _area.OnClick = value;
        get => _area.OnClick;
    }
    
    public UIButton(UIHost host, string text, Action? action = null) : base(host, GetMinimalSize(host.Style, text))
    {
        _area.OnClick = action;
        _text = text;
        AddArea(_area);
    }

    public override void UpdateLayout()
    {
        _area.Rect = Rect;
    }

    public override void Draw()
    {
        var style = _area.IsHovered ? Host.Style.HoveredButton : Host.Style.NormalButton;
        Raylib.DrawRectangleRec(Rect, style.BgColor);
        Raylib.DrawTextEx(
            Host.Style.Font, _text, Rect.Position + style.TextOffset, 
            Host.Style.FontSize, Host.Style.FontSpacing, style.TextColor
        );
    }
}