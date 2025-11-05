using System.Numerics;

namespace deGUISpace;

public sealed class RectGUIArea
{
    public Anchor Anchor;
    public string Name;

    public RectGUIArea? Parent = null;
    
    public int StartXRaw;
    public int StartYRaw;

    public int WidthRaw;
    public int HeightRaw;

    public int AdaptedX
    {
        get
        {
            return GUIUtil.AdaptX(StartXRaw);
        }
    }

    public int AdaptedY
    {
        get
        {
            return GUIUtil.AdaptY(StartYRaw);
        }
    }

    public int AdaptedWidth
    {
        get
        {
            if (WidthRaw == deGUI.STRETCH)
            {
                if(Parent == null)
                {
                    return deGUI.ScreenWidth;
                }
                else
                {
                    return Parent.AdaptedWidth;
                }
            }
            
            return GUIUtil.AdaptWidth(WidthRaw);
        }
    }
    public int AdaptedHeight
    {
        get
        {
            if (HeightRaw == deGUI.STRETCH)
            {
                if(Parent == null) return deGUI.ScreenHeight;
                else
                {
                    return Parent.AdaptedHeight;
                }
            }
            
            return GUIUtil.AdaptHeight(HeightRaw);
        }
    }

    public bool Fit(float x, float y)
    {
        Vector2 position = GUIUtil.AnchorPosition(this);
        Vector2 endPosition = new Vector2(position.X + AdaptedWidth, position.Y + AdaptedHeight);
        return x > position.X && y > position.Y && x < endPosition.X && y < endPosition.Y;
    }

    public bool Fit(Vector2 position) => Fit(position.X, position.Y);

    public RectGUIArea(Anchor anchor, int startX, int startY, int width, int height) 
    {
        Anchor = anchor;
        StartXRaw = startX;
        StartYRaw = startY;
        WidthRaw = width;
        HeightRaw = height;
    }
}