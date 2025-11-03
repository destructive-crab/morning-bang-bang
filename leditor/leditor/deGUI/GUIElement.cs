namespace deGUISpace;

public abstract class GUIElement
{
    public bool Active = true;
    
    public RectGUIArea GUIArea { get; set; }

    public GUIElement? Parent { get; private set; } = null;
    public GUIElement[] GUIElementChildren => children.ToArray();
    private readonly List<GUIElement> children = new();

    public void SetParent(GUIElement element)
    {
        Parent = element;
        GUIArea.Parent = element.GUIArea;

        Parent.AddChild(this);
        
        if (Parent == null)
        {
            deGUI.MarkAsRoot(this);
        }
        else
        {
            deGUI.RemoveFromRoots(this);
        }
    }

    public void AddChild(GUIElement element)
    {
        children.Add(element);

        if (element.Parent != this)
        {
            element.SetParent(this);
        }
    }

    public void RemoveChild(GUIElement element)
    {
        children.Remove(element);
        element.SetParent(null);
    }
    
    public virtual void Hide()
    {
        Active = false;
    }

    public virtual void Show()
    {
        Active = true;
    }
}