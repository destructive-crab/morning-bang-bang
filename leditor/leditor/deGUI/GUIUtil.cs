using System.Numerics;
using leditor.root;

namespace deGUISpace;

public class GUIUtil
{
    public static int AdaptX(int x) => (int)(x * deGUI.WCF);
    public static int AdaptY(int y) => (int)(y * deGUI.HCF);

    public static int AdaptWidth(int width) => (int)(width * deGUI.SCALE_COFF);
    public static int AdaptHeight(int height) => (int)(height * deGUI.SCALE_COFF);

    public static Vector2 AdaptV2(Vector2 v) => new Vector2(AdaptX((int)v.X), AdaptY((int)v.Y));
    public static Vector2 AdaptV2(int x, int y) => AdaptV2(new Vector2(x, y));
    
    public static Vector2 AnchorPosition(GUIElement guiElement)
    {
        return AnchorPosition(guiElement.GUIArea);
    }

    public static Vector2 AnchorPosition(RectGUIArea rectArea)
    {
        if(rectArea.Parent != null)
        {
            Vector2 startPosition = AnchorPosition(rectArea.Parent);
            Vector2 adaptedScale = new Vector2(rectArea.Parent.AdaptedWidth, rectArea.Parent.AdaptedHeight);
            int endx = (int)(startPosition.X + adaptedScale.X);
            int endy = (int)(startPosition.Y + adaptedScale.Y);
            
            Vector2 res = AnchorPosition(rectArea, (int)startPosition.X, (int)startPosition.Y, endx, endy);
            return res;
        }
        
        return AnchorPosition(rectArea, 0, 0, App.WindowHandler.Width, App.WindowHandler.Height);
    }

    public static Vector2 AnchorPosition(RectGUIArea rectArea, int startX, int startY, int w, int h)
    {
        int x = rectArea.AdaptedX;
        int y = rectArea.AdaptedY;
        
        int xScale = rectArea.AdaptedWidth;
        int yScale = rectArea.AdaptedHeight;

        switch (rectArea.Anchor)
        {
            case Anchor.Center:
                return new Vector2(w / 2 + x - xScale / 2, h / 2 + y - yScale / 2);
            
            case Anchor.CenterTop:
                return new Vector2(w/2 + x - xScale/2, startY + y);
            
            case Anchor.CenterBottom:
                return new Vector2(w/2 + x - xScale/2, h + y - yScale);
            
            case Anchor.RightCenter:
                return new Vector2(w + x - xScale, h/2 + y - yScale / 2);
            
            case Anchor.LeftCenter:
                return new Vector2(startX + x, h/2 + y - yScale / 2);
            
            case Anchor.LeftTop:
                return new Vector2(startX + x, startY + y);
            
            case Anchor.LeftBottom:
                return new Vector2(startX + x, h + y - yScale);
            
            case Anchor.RightTop:
                return new Vector2(w + x - xScale, startY + y);
            
            case Anchor.RightBottom:
                return new Vector2(w + x - xScale, h + y - yScale);
        }

        return Vector2.Zero;
    }
}