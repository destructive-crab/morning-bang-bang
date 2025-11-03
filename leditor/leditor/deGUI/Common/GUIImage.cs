using Raylib_cs;

namespace deGUISpace;

public class GUIImage : GUIElement
{
    public RectGUIArea GUIArea;
    public Texture2D Texture;

    public GUIElement Parent { get; set; }
    public bool Active { get; private set; }

    public GUIImage(RectGUIArea guiArea, Texture2D texture)
    {
        GUIArea = guiArea;
        Texture = texture;
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