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
    
    private readonly RectangleShape backgroundShape;
    private readonly RectangleShape backgroundOutlineShape;
    private readonly RectangleShape cursorShape;
    private readonly RectangleShape selectionShape;
    private readonly Text textObject;

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
        this.textObject = text;

        backgroundShape = new RectangleShape();
        backgroundOutlineShape = new RectangleShape();
        
        backgroundShape.FillColor = host.Style.EntryBackgroundColor();
        backgroundOutlineShape.FillColor = host.Style.OutlineColor();

        selectionShape = new RectangleShape();
        selectionShape.FillColor = new Color(0xFFFFFF70);
        
        cursorShape = new RectangleShape
        {
            Size = new Vector2f(host.Style.CursorWidth(), host.Style.FontSize()+2),
            FillColor = host.Style.CursorColor()
        };

        clickArea.OnRightMouseButtonClick += OnAreaClicked;

        Var.OnSet += OnVarUpdate;
    }

    public override void Draw(RenderTarget target)
    {
        if(isActive)
        {
            target.Draw(backgroundOutlineShape);
        }

        target.Draw(backgroundShape);

        Utils.CopyView(target.GetView(), origView);
        target.SetView(view);
        
        target.Draw(textObject);

        if (HasSelection())
        {
            target.Draw(selectionShape);
        }

        if (isActive && DateTime.Now.Ticks - lastTicks >= 5000000)
        {
            lastTicks = DateTime.Now.Ticks;
            draw = !draw;
        }
        
        if (isActive && draw)
        {
            target.Draw(cursorShape);
        }

        target.SetView(origView);
    }

    public override void ProcessClicks() => Host.Areas.Process(clickArea);

    private void OnAreaClicked()
    {
        Host.SetActive(this);
        isActive = true;
        draw = true;
        lastTicks = DateTime.Now.Ticks;
        CursorPosition = textObject.DisplayedString.Length;
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
        if (c == '\b' && textObject.DisplayedString.Length == 0) return;
        
        if (HasSelection())
        {
            textObject.DisplayedString = textObject.DisplayedString.Remove(GetSelectionStart(), GetAbsSelectionLength());
            CursorPosition = GetSelectionStart();
            ResetSelection();
            
            if (c != '\b') AppendCharacter(c);
        }
        else
        {
            if (c == '\b')
            {
                textObject.DisplayedString = textObject.DisplayedString.Remove(cursorPosition-1, 1);
                CursorPosition--;
            }
            else
            {
                textObject.DisplayedString = textObject.DisplayedString.Insert(cursorPosition, c.ToString());
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
                if (cursorPosition != textObject.DisplayedString.Length)
                {
                    if (HasSelection())
                    {
                        AppendCharacter('\b');
                    }
                    else
                    {
                        textObject.DisplayedString = textObject.DisplayedString.Remove(cursorPosition, 1);   
                    }
                }
                break;
            case Keyboard.Key.Home:
                CursorPosition = 0;
                break;
            case Keyboard.Key.End:
                CursorPosition = textObject.DisplayedString.Length;
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
        UpdateVar(textObject.DisplayedString);
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

        textObject.DisplayedString = val;
    }

    private void UpdateCursor()
    {
        cursorPosition = int.Clamp(cursorPosition, 0, textObject.DisplayedString.Length);
        var positionX = textObject.GetXPositionOfCharacter(cursorPosition);

        cursorShape.Position = new Vector2f(positionX, backgroundShape.Position.Y);

        positionX -= textObject.Position.X;
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
    private void UpdateSelection()
    {
        selectionLength = int.Clamp(selectionLength, -selectionStart, textObject.DisplayedString.Length-selectionStart);
        string sub = textObject.DisplayedString;
        
        float startPosition = textObject.GetXPositionOfCharacter(selectionStart);
        float endPosition = textObject.GetXPositionOfCharacter(selectionStart+selectionLength);
        
        float length = Math.Abs(startPosition - endPosition);

        float selectionX = 0;
        
        if (startPosition < endPosition) selectionX = startPosition;
        else selectionX = endPosition;
        
        selectionShape.Size = new Vector2f(length, cursorShape.Size.Y);
        selectionShape.Position = new Vector2f(selectionX, cursorShape.Position.Y);
        
        Console.WriteLine(selectionLength + " " + startPosition + " " + endPosition + " " + length);
    }

    protected override void OnHostSizeChangedIm(Vector2f newSize)
    {
        view.Center = Rect.Position + Rect.Size / 2;
        view.Size = Rect.Size;
        view.Viewport = new FloatRect(
            Rect.Left / Host.Size.X - 0.0000001f,
            Rect.Top / Host.Size.Y,
            Rect.Width / Host.Size.X,
            Rect.Height / Host.Size.Y
        );
    }

    protected override void UpdateLayoutIm()
    {
        int outline = Host.Style.BaseOutline() / 2;
        
        backgroundShape.Size = Rect.Size - new Vector2f(outline, outline);
        backgroundShape.Position = Rect.Position + new Vector2f(outline, outline);
        
        backgroundOutlineShape.Size = Rect.Size + new Vector2f(outline, outline);
        backgroundOutlineShape.Position = Rect.Position;

        cursorShape.Size = new Vector2f(textObject.LetterSpacing+1, backgroundShape.Size.Y);
        
        textObject.Position = backgroundShape.Position + new Vector2f(Host.Style.BoxSizeX()/2f, Host.Style.BoxSizeY()/2f-2f);
        
        UpdateCursor();

        clickArea.Rect = Rect;
    }

    protected bool HasSelection() => selectionLength != 0;

    protected int GetSelectionStart()
    {
        if (selectionLength < 0)
        {
            return selectionStart + selectionLength;
        }

        return selectionStart;
    }

    protected int GetSelectionEnd()
    {
        if (selectionLength < 0)
        {
            return selectionStart;
        }

        return selectionStart + selectionLength;
    }

    protected void ResetSelection()
    {
        selectionLength = 0;
    }

    protected int GetAbsSelectionLength() => Math.Abs(selectionLength);
}