namespace deGUISpace;

public class GUIGroup : GUIElement
{
    public GUIGroup(RectGUIArea area, params GUIElement[] elements)
    {
        GUIArea = area;
        foreach (GUIElement element in elements)
        {
            AddChild(element);
        }
    }

    public void Hide()
    {
        Active = false;
    }

    public void Show()
    {
        Active = true;
    }
}