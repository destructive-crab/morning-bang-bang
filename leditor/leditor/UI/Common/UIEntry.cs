using leditor.root;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace leditor.UI;

public class UIEntry : AUIElement
{
    public UIVar<string> Var { get; private set; }
    
    private readonly ClickArea clickArea = new(new FloatRect());
    
    private readonly View view = new();
    private readonly View origView = new();
    
    private readonly RectangleShape background;
    private readonly RectangleShape backgroundOutline;
    private readonly RectangleShape cursor;
    private readonly RectangleShape selection;
    private readonly Text text;

    private bool isActive;
    private bool draw = false;
    
    private int cursorPosition;

    private int selectionStart;
    private int selectionLength;
    
    private long lastTicks = 0;

    private string? origValue;
    private float xOffset;

    private int CursorPosition
    {
        get => cursorPosition;
        set
        {
            if (App.WindowHandler.InputsHandler.IsKeyPressed(Keyboard.Key.LShift) ||
                App.WindowHandler.InputsHandler.IsKeyPressed(Keyboard.Key.RShift))
            {
                if (selectionLength == 0) selectionStart = cursorPosition;
                
                selectionLength += (value - cursorPosition);
                UpdateSelection();
            }
            else
            {
                selectionStart = value;
                selectionLength = 0;
            }
            cursorPosition = value;
            UpdateCursor();
        }
    }

    public UIEntry(UIHost host, UIVar<string> var, int minX = 0)
        : base(host, host.Fabric.MakeTextOut("X", out var text) + new Vector2f(host.Style.BoxSizeX()+minX, host.Style.BoxSizeY()))
    {
        text.DisplayedString = var.Value;
        Var = var;
        this.text = text;

        background = new RectangleShape();
        backgroundOutline = new RectangleShape();
        
        background.FillColor = host.Style.EntryBackgroundColor();
        backgroundOutline.FillColor = host.Style.OutlineColor();

        selection = new RectangleShape();
        selection.FillColor = new Color(0xFFFFFF70);
        
        cursor = new RectangleShape
        {
            Size = new Vector2f(host.Style.CursorWidth(), host.Style.FontSize()+2),
            FillColor = host.Style.CursorColor()
        };

        clickArea.OnRightMouseButtonClick += OnAreaClicked;

        Var.OnSet += OnVarUpdate;
    }

    public override void ProcessClicks() => Host.Areas.Process(clickArea);
    private void OnAreaClicked()
    {
        Host.SetActive(this);
        isActive = true;
        draw = true;
        lastTicks = DateTime.Now.Ticks;
        CursorPosition = text.DisplayedString.Length;
    }
    public override void OnTextEntered(string text) 
    {
        for (var i = 0; i < text.Length; i++)
        {
            AppendCharacter(text[i]);
        }
    }

    private void AppendCharacter(char c)
    {
        if (selectionLength != 0)
        {
            int removeStart = 0;
                
            if (selectionLength > 0) removeStart = selectionStart;
            else                     removeStart = selectionStart + selectionLength;
                
            text.DisplayedString = text.DisplayedString.Remove(removeStart, Math.Abs(selectionLength));
            
            if (c == '\b')
            {
            }
            else
            {
                text.DisplayedString = text.DisplayedString.Insert(selectionStart, c.ToString());
            }

            selectionLength = 0;
            CursorPosition = removeStart;
        }
        else
        {
            if (c == '\b')
            {
                text.DisplayedString = text.DisplayedString.Remove(cursorPosition-1);
                CursorPosition--;
            }
            else
            {
                text.DisplayedString = text.DisplayedString.Insert(cursorPosition, c.ToString());
                CursorPosition++;
            }
        }
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
                if (cursorPosition != text.DisplayedString.Length)
                {
                    text.DisplayedString = text.DisplayedString.Remove(cursorPosition, 1);
                }
                break;
            case Keyboard.Key.Home:
                CursorPosition = 0;
                break;
            case Keyboard.Key.End:
                CursorPosition = text.DisplayedString.Length;
                break;
            case Keyboard.Key.Enter:
                Host.SetActive(null);
                break;
        }
    }
    public override void OnMouseClick(Vector2f pos)
    {
        if (clickArea.IsHovered) return;
        
        Host.SetActive(null);
    }
    
    public override void Deactivate()
    {
        UpdateVar(text.DisplayedString);
        isActive = false;
        CursorPosition = 0;
    }

    private void UpdateVar(string text)
    {
        origValue = text;
        Var.Value = text;
        origValue = null;
    }
    private void OnVarUpdate(string val)
    {
        if (origValue == val)
        {
            return;
        }

        text.DisplayedString = val;
    }
    
    private void UpdateCursor()
    {
        cursorPosition = int.Clamp(cursorPosition, 0, text.DisplayedString.Length);
        var positionX = GetXPositionInText(text, cursorPosition);

        cursor.Position = new Vector2f(positionX, background.Position.Y);

        positionX -= text.Position.X;
        float inner = positionX - xOffset;
        
        if (!(inner < 0))
        {
            if (inner > Rect.Width - 2)
            {
                xOffset = positionX - (Rect.Width - 2);
            }
        }
        else
        {
            xOffset = positionX;
        }

        view.Center = Rect.Position + Rect.Size / 2 + new Vector2f(xOffset, 0);
    }

    private float GetXPositionInText(Text providedText, int position)
    {
        string sub = this.text.DisplayedString[..position];
        float positionX = 0f;

        uint previousSymbol = 0u;
        foreach (char symbol in sub)
        {   
            positionX += 
                providedText.Font.GetGlyph(symbol, providedText.CharacterSize, true, providedText.OutlineThickness).Advance + 
                providedText.Font.GetBoldKerning(previousSymbol, symbol, providedText.CharacterSize);
            
            previousSymbol = symbol;
        }

        return providedText.Position.X + positionX;
    }

    private void UpdateSelection()
    {
        selectionLength = int.Clamp(selectionLength, -selectionStart, text.DisplayedString.Length-selectionStart);
        string sub = text.DisplayedString;
        
        float startPosition = GetXPositionInText(text, selectionStart);
        float endPosition = GetXPositionInText(text, selectionStart+selectionLength);
        
        float length = Math.Abs(startPosition - endPosition);

        float selectionX = 0;
        
        if (startPosition < endPosition) selectionX = startPosition;
        else selectionX = endPosition;
        
        selection.Size = new Vector2f(length, cursor.Size.Y);
        selection.Position = new Vector2f(selectionX, cursor.Position.Y);
        
        Console.WriteLine(selectionLength + " " + startPosition + " " + endPosition + " " + length);
    }
    
    public override void UpdateLayout()
    {
        view.Center = Rect.Position + Rect.Size / 2;
        view.Size = Rect.Size;
        view.Viewport = new FloatRect(
            Rect.Left / Host.Size.X - 0.0000001f,
            Rect.Top / Host.Size.Y,
            Rect.Width / Host.Size.X,
            Rect.Height / Host.Size.Y
        );
        
        int outline = Host.Style.BaseOutline() / 2;
        
        background.Size = Rect.Size - new Vector2f(outline, outline);
        background.Position = Rect.Position + new Vector2f(outline, outline);
        
        backgroundOutline.Size = Rect.Size + new Vector2f(outline, outline);
        backgroundOutline.Position = Rect.Position;

        cursor.Size = new Vector2f(text.LetterSpacing+1, background.Size.Y);
        
        text.Position = background.Position + new Vector2f(Host.Style.BoxSizeX()/2f, Host.Style.BoxSizeY()/2f-2f);
        
        UpdateCursor();

        clickArea.Rect = Rect;
    }

    public override void Draw(RenderTarget target)
    {
        if(isActive) target.Draw(backgroundOutline);
        target.Draw(background);

        Utils.CopyView(target.GetView(), origView);
        target.SetView(view);
        
        target.Draw(text);

        if (selectionLength != 0)
        {
            target.Draw(selection);
        }

        if (isActive && DateTime.Now.Ticks - lastTicks >= 5000000)
        {
            lastTicks = DateTime.Now.Ticks;
            draw = !draw;
        }
        
        if (isActive && draw)
        {
            target.Draw(cursor);
        }

        target.SetView(origView);
    }
}