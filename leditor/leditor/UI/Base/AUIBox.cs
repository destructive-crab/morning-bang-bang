using SFML.System;

namespace leditor.UI;

public abstract class AUIBox(UIHost host, Vector2f minimalSize) : AUIElement(host, minimalSize)
{
    public abstract IEnumerable<AUIElement> GetChildren();

    public abstract void RemoveChild(AUIElement child);

    public void RemoveAllChildren()
    {
        IEnumerable<AUIElement> all = GetChildren();
        all = new List<AUIElement>(all);
        
        foreach (AUIElement element in all)
        {
            RemoveChild(element);
        }
    }

    protected abstract void UpdateMinimalSize();

    public override void ProcessClicks()
    {
        var children = GetChildren();
        
        foreach (var child in children)
            Host.ClickHandlersStack.Push(child.ProcessClicks);
    }

    public void OnChildUpdate()
    {
        var size = MinimalSize;
        UpdateMinimalSize();
        
        if (size == MinimalSize)
            Host.UpdateActionsQueue.Enqueue(UpdateLayout);
    }
}