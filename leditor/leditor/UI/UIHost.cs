using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using leditor.root;
using Raylib_cs;

namespace leditor.UI;

public class UIHost(UIStyle style)
{
    public AUIElement? Root;

    private bool AssertRoot([MaybeNullWhen(false)] out AUIElement root)
    {
        root = Root;
        
        if (root != null) return true;
        
        Logger.Warn("UI Root in null!");
        return false;

    }
    
    internal readonly Queue<AUIElement> RectUpdateQueue = [];
    
    public void SetSize(Vector2 size)
    {
        if (!AssertRoot(out var root)) return;
        
        root.Rect = new Rectangle(Vector2.Zero, size);
        while (RectUpdateQueue.TryDequeue(out var element))
            element.OnRectUpdate();
    }

    internal readonly Queue<AUIElement> DrawQueue = [];
    
    public void Draw()
    {
        if (!AssertRoot(out var root)) return;
        
        root.Draw();
        while (DrawQueue.TryDequeue(out var element))
            element.Draw();
    }

    public UIStyle Style = style;
}