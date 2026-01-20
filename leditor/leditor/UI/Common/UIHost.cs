using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using leditor.root;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace leditor.UI;

public class UIHost
{
    public AUIElement? Root;
    public ClickAreasController Areas = new();
    public View View = new();
    public Vector2f Size;

    public UIFabric Fabric;

    public UIHost(UIStyle style, Vector2f size)
    {
        Style = style;
        Fabric = new UIFabric(this);
        SetSize(size);
    }

    public void SetRoot(AUIElement root)
    {
        Root = root;
        SetSize(Size);
    }
    private bool AssertRoot(
        [MaybeNullWhen(false)] out AUIElement root,
        [CallerFilePath] string filePath = "", 
        [CallerLineNumber] int lineNumber = 0
    ) {
        root = Root;
        
        if (root != null) return true;
        
        //Logger.Warn("UI Root in null!", filePath, lineNumber);
        return false;
    }

    public void SetSize(Vector2f size)
    {
        View.Size = size;
        View.Center = size / 2;
        Size = size;
        if (AssertRoot(out var root))
            root.Rect = new FloatRect(new Vector2f(0,0), size);
    }

    internal Queue<Action> UpdateActionsQueue = [];

    private void ProcessUpdateActions()
    {
        while (UpdateActionsQueue.TryDequeue(out Action? action))
        {
            action?.Invoke();
        }
    }

    public readonly Stack<Action> ClickHandlersStack = [];

    public void Update(RenderWindow window)
    {
        if (!AssertRoot(out var root)) return;
        
        Areas.Begin(Utils.VecI2F(Mouse.GetPosition(window)));
        root.ProcessClicks();
        while (ClickHandlersStack.TryPop(out var action))
            action();
        Areas.End();
        
        ProcessUpdateActions();
    }

    public delegate void DrawAction(RenderTarget target);

    public readonly Stack<DrawAction> DrawStack = [];

    public void Draw(RenderTarget target)
    {
        if (!AssertRoot(out AUIElement? root)) return;

        target.SetView(View);
        root.Draw(target);
        
        while (DrawStack.TryPop(out DrawAction? draw))
        {
            draw(target);
        }
    }

    public readonly UIStyle Style;

    private AUIElement? _active;

    public void SetActive(AUIElement? element)
    {
        _active?.Deactivate();
        _active = element;
    }

    public void OnTextEntered(string text) => _active?.OnTextEntered(text);
    public void OnKeyPressed(Keyboard.Key key) => _active?.OnKeyPressed(key);
    public void OnMouseClick(Vector2f pos) => _active?.OnMouseClick(pos);
}