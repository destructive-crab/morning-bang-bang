using System.Text;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace leditor.UI;

public class UIEntry : AUIElement
{
    public UIVar<string> Var { get; private set; }
    
    private View _view = new();

    private readonly RectangleShape background;

    private ClickArea _area = new(new FloatRect());

    private Text _text;

    private bool _active;

    private RectangleShape _cursor;
    private int _cursorPosition;

    public int CursorPosition
    {
        get => _cursorPosition;
        set
        {
            _cursorPosition = value;
            UpdateCursor();
        }
    }

    public UIEntry(UIHost host, UIVar<string> var, int minX = 0) : base(host, host.Fabric.MakeTextOut("X", out var text) + new Vector2f(host.Style.BoxSizeX+minX, host.Style.BoxSizeY))
    {
        text.DisplayedString = var.Value;
        Var = var;
        _text = text;

        background = new RectangleShape();
        background.FillColor = host.Style.EntryBackgroundColor;
        
        _cursor = new RectangleShape
        {
            Size = new Vector2f(host.Style.CursorWidth, host.Style.FontSize+2),
            FillColor = host.Style.CursorColor
        };

        _area.OnRightMouseButtonClick += OnAreaClicked;

        Var.OnSet += OnVarUpdate;
    }

    public override void ProcessClicks()
        => Host.Areas.Process(_area);

    private void OnAreaClicked()
    {
        Host.SetActive(this);
        _active = true;
        CursorPosition = _text.DisplayedString.Length;
    }

    public override void Deactivate()
    {
        UpdateVar(_text.DisplayedString);
        _active = false;
        CursorPosition = 0;
    }

    public override void OnTextEntered(string text)
    {
        var begin = new StringBuilder(_text.DisplayedString[.._cursorPosition]);
        var end = new StringBuilder(_text.DisplayedString[_cursorPosition..]);
        
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\b')
            {
                var remove = 0;
                while (i < text.Length && text[i] == '\b')
                {
                    remove++;
                    i++;
                }

                remove = int.Min(remove, begin.Length);
                begin.Remove(begin.Length - remove, remove);
            }
            else
            {
                var start = i;
                var subEnd = i;
                while (i < text.Length && text[i] != '\b')
                {
                    i++;
                    subEnd = i;
                }

                begin.Append(text[start..subEnd]);
            }
        }
        
        var pos = begin.Length;
        begin.Append(end);

        _text.DisplayedString = begin.ToString();
        CursorPosition = pos;
    }

    public override void OnKeyPressed(Keyboard.Key key)
    {
        switch (key)
        {
            case Keyboard.Key.Left:
                CursorPosition -= 1;
                break;
            case Keyboard.Key.Right:
                CursorPosition += 1;
                break;
            case Keyboard.Key.Delete:
                if (_cursorPosition != _text.DisplayedString.Length)
                    _text.DisplayedString = _text.DisplayedString.Remove(_cursorPosition, 1);
                break;
            case Keyboard.Key.Enter:
                Host.SetActive(null);
                break;
        }
    }

    private string? _origValue;

    private void UpdateVar(string text)
    {
        _origValue = text;
        Var.Value = text;
        _origValue = null;
    }

    private void OnVarUpdate(string val)
    {
        if (_origValue == val) return;
        
        _text.DisplayedString = val;
    }

    public override void OnMouseClick(Vector2f pos)
    {
        if (_area.IsHovered) return;
        
        Host.SetActive(null);
    }

    private float _xOffset;
    
    private void UpdateCursor()
    {
        _cursorPosition = int.Clamp(_cursorPosition, 0, _text.DisplayedString.Length);
        
        var sub = _text.DisplayedString[.._cursorPosition];
        var position = new Vector2f();

        var prevSymbl = 0u;
        foreach (var symbl in sub)
        {   
            position.X += 
                _text.Font.GetGlyph(symbl, _text.CharacterSize, true, _text.OutlineThickness).Advance + 
                _text.Font.GetBoldKerning(prevSymbl, symbl, _text.CharacterSize);
            prevSymbl = symbl;
        }

        _cursor.Position = position + Rect.Position + new Vector2f(1, 1);
        _cursor.Position = new Vector2f(_cursor.Position.X, _text.Position.Y);

        var inner = position.X - _xOffset;
        if (inner < 0)
            _xOffset = position.X;
        else if (inner > Rect.Width - 2)
            _xOffset = position.X - (Rect.Width - 2);
        
        _view.Center = Rect.Position + Rect.Size / 2 + new Vector2f(_xOffset, 0);
    }
    
    public override void UpdateLayout()
    {
        _view.Center = Rect.Position + Rect.Size / 2;
        _view.Size = Rect.Size;
        _view.Viewport = new FloatRect(
            Rect.Left / Host.Size.X - 0.0000001f,
            Rect.Top / Host.Size.Y,
            Rect.Width / Host.Size.X,
            Rect.Height / Host.Size.Y
        );

        background.Size = Rect.Size;
        background.Position = Rect.Position;
        _text.Position = Rect.Position + new Vector2f(Host.Style.BoxSizeX/2, Host.Style.BoxSizeY/2-2);
        
        UpdateCursor();

        _area.Rect = Rect;
    }

    private View _origView = new();
    
    public override void Draw(RenderTarget target)
    {
        target.Draw(background);

        Utils.CopyView(target.GetView(), _origView);
        target.SetView(_view);
        
        target.Draw(_text);
        
        if (_active)
            target.Draw(_cursor);
        
        target.SetView(_origView);
    }
}