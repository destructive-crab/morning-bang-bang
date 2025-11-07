using System.Numerics;
using Raylib_cs;

namespace leditor.UI;

public abstract class AUIElement(UIHost host, Vector2 minimalSize)
{
    public AUIBox? Parent;

    public void Destroy()
        => Parent?.RemoveChild(this);

    protected readonly UIHost Host = host;
    
    private Vector2 _minimalSize = minimalSize;

    public Vector2 MinimalSize
    {
        get => _minimalSize;
        protected set
        {
            _minimalSize = value;
            
            Host.NeedLayoutUpdate = true;
            if (Parent != null)
                Host.UpdateActionsQueue.Enqueue(Parent.UpdateMinimalSize);
        }
    }

    private Rectangle _rect;
    public Rectangle Rect
    {
        get => _rect; 
        set
        {
            _rect = new Rectangle(
                value.X, value.Y,
                float.Max(MinimalSize.X, value.Width),
                float.Max(MinimalSize.X, value.Height)
            );
            
            Host.UpdateActionsQueue.Enqueue(UpdateLayout);
        }
    } 

    public abstract void UpdateLayout();

    public abstract void Draw();
}