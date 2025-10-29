using System.Numerics;

namespace leditor.root;

public class ButtonGroup
{
    public Anchor Anchor = Anchor.LeftBottom;
    
    public int XSpace;
    public int YSpace;
    
    public Vector2 offset;
    public List<Button> buttons;

    public int LimitHorizontal = -1;
    public int LimitVertical = -1;

    public ButtonGroup(int offsetX, int offsetY, Anchor anchor, params Button[] group)
    {
        offset = new Vector2(offsetX, offsetY);
        Anchor = anchor;
        
        buttons = new List<Button>(group);

        foreach (Button button in buttons)
        {
            button.Anchor = Anchor;

            button.OffsetX = offsetX;
            button.OffsetY = offsetY;
        }
    }
}