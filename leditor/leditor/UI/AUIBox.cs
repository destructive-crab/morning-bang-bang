using System.Numerics;

namespace leditor.UI;

public abstract class AUIBox(UIHost host, Vector2 minimalSize) : AUIElement(host, minimalSize)
{
    public abstract IEnumerable<AUIElement> GetChildren();
    
    public abstract void RemoveChild(AUIElement child);
    
    public abstract void UpdateMinimalSize();
}