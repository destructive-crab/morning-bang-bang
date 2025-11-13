using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using leditor.root;
using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIHost(UIStyle style)
{
    public AUIElement? Root;
    public ClickAreasController Areas = new();
    public View View;
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
        Size = size;
        if (AssertRoot(out var root))
            root.Rect = new FloatRect(new Vector2f(0,0), size);
    }
    
    internal Queue<Action> UpdateActionsQueue = [];
    internal bool NeedLayoutUpdate;

    private void ProcessUpdateActions()
    {
        while (UpdateActionsQueue.TryDequeue(out var action)) action();
    }
    
    public void Update(RenderWindow window)
    {
        Areas.Update(window);
        ProcessUpdateActions();

        if (NeedLayoutUpdate && AssertRoot(out var root))
        {
            NeedLayoutUpdate = false;
            root.UpdateLayout();
            ProcessUpdateActions();
        }
    }
    
    public delegate void DrawAction(RenderTarget target);
    public readonly Stack<DrawAction> DrawStack = [];
    
    public void Draw(RenderTarget target)
    {
        if (!AssertRoot(out var root)) return;

        View = new View(target.GetView());
        
        root.Draw(target);
        while (DrawStack.TryPop(out var draw))
            draw(target);
        
        target.SetView(View);
    }

    public readonly UIStyle Style = style;
}