using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using leditor.root;
using Raylib_cs;

namespace leditor.UI;

public class UIHost(UIStyle style)
{
    public AUIElement? Root;
    public ClickAreasController Areas = new();

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
    
    public void SetSize(Vector2 size)
    {
        if (AssertRoot(out var root))
            root.Rect = new Rectangle(Vector2.Zero, size);
    }
    
    internal Queue<Action> UpdateActionsQueue = [];
    internal bool NeedLayoutUpdate;

    private void ProcessUpdateActions()
    {
        while (UpdateActionsQueue.TryDequeue(out var action)) action();
    }
    
    public void Update()
    {
        Areas.Update();
        ProcessUpdateActions();

        if (NeedLayoutUpdate && AssertRoot(out var root))
        {
            NeedLayoutUpdate = false;
            root.UpdateLayout();
            ProcessUpdateActions();
        }
    }

    public readonly Stack<Action> DrawStack = [];
    
    public void Draw()
    {
        if (!AssertRoot(out var root)) return;
        
        root.Draw();
        while (DrawStack.TryPop(out var draw))
            draw();
    }

    public readonly UIStyle Style = style;
}