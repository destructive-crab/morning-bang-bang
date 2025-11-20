using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using leditor.root;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace leditor.UI;

public class UIHost(UIStyle style)
{
    public AUIElement? Root;
    public ClickAreasController Areas = new();
    public View View = new();
    public Vector2f Size;
    
    private bool AssertRoot(
        [MaybeNullWhen(false)] out AUIElement root,
        [CallerFilePath] string filePath = "", 
        [CallerLineNumber] int lineNumber = 0
    ) {
        root = Root;
        
        if (root != null) return true;
        
        Logger.Warn("UI Root in null!", filePath, lineNumber);
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
        while (UpdateActionsQueue.TryDequeue(out var action)) 
            action();
    }
    
    public void Update(RenderWindow window)
    {
        Areas.Update(window);
        ProcessUpdateActions();
    }
    
    public delegate void DrawAction(RenderTarget target);
    public readonly Stack<DrawAction> DrawStack = [];
    
    public void Draw(RenderTarget target)
    {
        if (!AssertRoot(out var root)) return;

        target.SetView(View);
        root.Draw(target);
        while (DrawStack.TryPop(out var draw))
            draw(target);
        
    }

    public readonly UIStyle Style = style;
    
    private AUIElement? _active;

    public void SetActive(AUIElement? element)
    {
        _active?.Deactivate();
        _active = element;
    }

    public void OnTextEntered(string text)
        => _active?.OnTextEntered(text);

    public void OnKeyPressed(Keyboard.Key key)
        => _active?.OnKeyPressed(key);
    
    public void OnMouseClick(Vector2f pos)
        => _active?.OnMouseClick(pos);
}