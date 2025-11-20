using System.Text;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace leditor.UI;

public class UIEntry : AUIElement
{
    public UIVar<string> Var { get; private set; }
    
    private View _view = new();
    private RectangleShape _rectangle = new()
    {
        FillColor = Color.White
    };

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

    public UIEntry(UIHost host, UIVar<string> var) : base(host, new Vector2f(host.Style.FontSize + 4, host.Style.FontSize + 2))
    {
        Var = var;
        _text = new Text
        {
            Font = host.Style.Font,
            DisplayedString = var.Value,
            FillColor = Color.Black,
            CharacterSize = host.Style.FontSize
        };
        _cursor = new RectangleShape
        {
            Size = new Vector2f(1, host.Style.FontSize),
            FillColor = Color.Black
        };

        _area.OnClick += OnAreaClicked;
        AddArea(_area);

        Var.OnSet += OnVarUpdate;
    }

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
        CursorPosition = val.Length;
    }

    public override void OnMouseClick(Vector2f pos)
    {
        if (_area.IsHovered) return;
        
        Host.SetActive(null);
    }

    private void UpdateCursor()
    {
        _cursorPosition = int.Clamp(_cursorPosition, 0, _text.DisplayedString.Length);
        
        var sub = _text.DisplayedString[.._cursorPosition];
        var position = Rect.Position + new Vector2f(1, 1);

        foreach (var symbl in sub)
            position.X += _text.Font
                .GetGlyph(symbl, _text.CharacterSize, false, 0)
                .Advance * _text.LetterSpacing;

        _cursor.Position = position;
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

        _rectangle.Size = Rect.Size;
        _rectangle.Position = Rect.Position;
        _text.Position = Rect.Position + new Vector2f(2, 1);
        
        UpdateCursor();

        _area.Rect = Rect;
    }

    public override void Draw(RenderTarget target)
    {
        var origView = target.GetView();
        target.SetView(_view);
        
        target.Draw(_rectangle);
        target.Draw(_text);
        
        if (_active)
            target.Draw(_cursor);
        
        target.SetView(origView);
    }
}