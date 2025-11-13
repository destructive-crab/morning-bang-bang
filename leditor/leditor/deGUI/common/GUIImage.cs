using SFML.Graphics;

namespace deGUISpace;

public class GUIImage : GUIElement
{
    public RectGUIArea GUIArea;
    public Texture Texture;

    public GUIElement Parent { get; set; }
    public bool Active { get; private set; }

    public GUIImage(RectGUIArea guiArea, Texture texture)
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