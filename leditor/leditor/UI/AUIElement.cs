using System.Numerics;
using Raylib_cs;

namespace leditor.UI;

public abstract class AUIElement(UIHost host, Vector2 minimalSize)
{
    public AUIBox? Parent;

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
                float.Max(MinimalSize.Y, value.Height)
            );
            
            Host.UpdateActionsQueue.Enqueue(UpdateLayout);
        }
    } 

    public abstract void UpdateLayout();

    public abstract void Draw();

    private readonly List<ClickArea> _areas = [];

    protected void AddArea(ClickArea area)
    {
        _areas.Add(area);
        Host.Areas.AddArea(area);
    }

    private void DestroyAreas()
    {
        foreach (var area in _areas)
            Host.Areas.RemoveArea(area);
        
        _areas.Clear();
    }
    
    public void Destroy()
    {
        Parent?.RemoveChild(this);
        DestroyAreas();
    }
    
    ~AUIElement()
        => DestroyAreas();
}