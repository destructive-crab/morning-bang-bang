using System.Numerics;

namespace leditor.root;

public class GUIUtil
{
    
    public static int AdaptX(int x) => (int)(x * deGUI.WCF);
    public static int AdaptY(int y) => (int)(y * deGUI.HCF);
    
    public static Vector2 AnchorPosition(AreaManager.RectGUIArea rectGui)
    {
        Vector2 position;

        int originalX = (int)rectGui.StartX;
        int originalY = (int)rectGui.StartY;

        int x = GUIUtil.AdaptX(originalX);
        int y = GUIUtil.AdaptY(originalY);
        
        int xScale = AdaptX(rectGui.Width);
        int yScale = AdaptY(rectGui.Height);

        int w = deGUI.ScreenWidth;
        int h = deGUI.ScreenHeight;
        
        switch (rectGui.Anchor)
        {
            case Anchor.Center:
                return new Vector2(w / 2 + x - xScale / 2, h / 2 + y - yScale / 2);
            
            case Anchor.CenterTop:
                return new Vector2(w/2 + x - xScale/2, 0 + y - yScale / 2);
            
            case Anchor.CenterBottom:
                return new Vector2(w/2 + x - xScale/2, h + y - yScale / 2);
            
            case Anchor.RightCenter:
                return new Vector2(w + x - xScale/2, h/2 + y - yScale / 2);
            
            case Anchor.LeftCenter:
                return new Vector2(0 + x - xScale/2, h/2 + y - yScale / 2);
            
            case Anchor.LeftTop:
                return new Vector2(0 + x - xScale/2, 0 + y - yScale / 2);
            
            case Anchor.LeftBottom:
                return new Vector2(0 + x - xScale/2, h + y - yScale / 2);
            
            case Anchor.RightTop:
                return new Vector2(w + x - xScale/2, 0 + y - yScale / 2);
            
            case Anchor.RightBottom:
                return new Vector2(w + x - xScale/2, h + y - yScale / 2);
        }

        return Vector2.Zero;
    }
    
    public static Vector2 AnchorPosition(Button button)
    {
        return AnchorPosition(button.GUIArea as AreaManager.RectGUIArea);
    }
}