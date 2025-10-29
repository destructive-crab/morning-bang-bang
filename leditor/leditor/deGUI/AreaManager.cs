using System.Numerics;
using Raylib_cs;

namespace leditor.root;

public sealed class AreaManager
{
    public abstract class GUIArea
    {
        public Anchor Anchor;
        public string Name;

        protected GUIArea(Anchor anchor, string name)
        {
            Name = name;
        }

        public abstract bool Fit(float x, float y);
    }
    public class RectGUIArea : GUIArea
    {
        public int StartX;
        public int StartY;

        public int Width;
        public int Height;

        public int EndX => StartX + Width;
        public int EndY => StartY + Height;

        public override bool Fit(float x, float y)
        {
            Vector2 position = GUIUtil.AnchorPosition(this);
            Vector2 endPosition = new Vector2(position.X + GUIUtil.AdaptX(Width), position.Y + GUIUtil.AdaptY(Height));
            return x > position.X && y > position.Y && x < endPosition.X && y < endPosition.Y;
        }

        public RectGUIArea(Anchor anchor, int startX, int startY, int width, int height, string name) : base(anchor, name)
        {
            Anchor = anchor;
            StartX = startX;
            StartY = startY;
            Width = width;
            Height = height;
        }
    }

    private List<GUIArea> areas = new();
    
    public RectGUIArea BindRect(Anchor anchor, int x, int y, int w, int h, string name)
    {
        RectGUIArea rectGuiArea = new RectGUIArea(anchor, x, y, w, h, name);
        rectGuiArea.Anchor = anchor;
        
        areas.Add(rectGuiArea);
        return rectGuiArea;
    }

    public void Process()
    {

    }

    public void DrawDebugLayout()
    {
        foreach (GUIArea area in areas)
        {
            if (area is RectGUIArea rectArea)
            {
                Color color = Color.Green;
                color.A = 90;
                Vector2 position = GUIUtil.AnchorPosition(rectArea);
                Raylib.DrawRectangle((int)position.X, (int)position.Y, (int)(rectArea.Width * deGUI.WCF), (int)(rectArea.Height * deGUI.HCF), color);
            }
        }
    }
}