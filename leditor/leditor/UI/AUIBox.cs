using SFML.System;

namespace leditor.UI;

public abstract class AUIBox(UIHost host, Vector2f minimalSize) : AUIElement(host, minimalSize)
{
    public abstract IEnumerable<AUIElement> GetChildren();
    
    public abstract void RemoveChild(AUIElement child);

    protected abstract void UpdateMinimalSize();

    protected override void OnClickViewUpdate()
    {
        foreach (var child in GetChildren())
            child.SetClickView(ClickView);
    }

    public void OnChildUpdate()
    {
        var size = MinimalSize;
        UpdateMinimalSize();
        
        if (size == MinimalSize)
            Host.UpdateActionsQueue.Enqueue(UpdateLayout);
    }
}