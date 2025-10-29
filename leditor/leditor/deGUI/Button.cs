using Raylib_cs;

namespace leditor.root;

public class Button
{
    public Anchor Anchor;
    
    public int OffsetX;
    public int OffsetY; 
       
    public AreaManager.GUIArea GUIArea;
    
    public int ScaleX;
    public int ScaleY;
    
    //contents
    public string Label = "";
    
    public Texture2D Texture;
    public Color Color = Color.White;

    //interactions
    private Action RightCallbacks;
    private Action LeftCallbacks;

    public void ApplyArea(AreaManager.GUIArea area)
    {
        GUIArea = area;
    }
    
    public Button(int offsetX, int offsetY, int scaleX, int scaleY, string label)
    {
        OffsetX = offsetX;
        OffsetY = offsetY;
        ScaleX = scaleX;
        ScaleY = scaleY;
        Label = label;
    }

    public Button(Anchor anchor, int offsetX, int offsetY, int scaleX, int scaleY, string label = "")
    {
        Anchor = anchor;
        OffsetX = offsetX;
        OffsetY = offsetY;
        ScaleX = scaleX;
        ScaleY = scaleY;
        Label = label;
    }

    public void AddCallback(Action right, Action left)
    {
        if(right != null) RightCallbacks += right;
        if(left != null) LeftCallbacks += left;
    }

    public void InvokeRightClick()
    {
        RightCallbacks?.Invoke();
    }

    public void InvokeLeftClick()
    {
        LeftCallbacks?.Invoke();
    }
}