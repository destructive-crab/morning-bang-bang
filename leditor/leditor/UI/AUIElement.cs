using System.Numerics;
using Raylib_cs;

namespace leditor.UI;

public abstract class AUIElement(UIHost host, Vector2 minimalSize)
{
    public UIHost Host = host;

    public Vector2 MinimalSize { get; internal set; } = minimalSize;

    private Rectangle _rect;
    public Rectangle Rect
    {
        get => _rect; 
        set
        {
            _rect = value;
            _rect.Width = float.Max(MinimalSize.X, _rect.Width);
            _rect.Height = float.Max(MinimalSize.X, _rect.Height);
            
            Host.RectUpdateQueue.Enqueue(this);
        }
    }

    internal abstract void OnRectUpdate();

    public abstract void Draw();
}