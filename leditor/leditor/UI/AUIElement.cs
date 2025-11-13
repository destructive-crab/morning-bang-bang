using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public abstract class AUIElement(UIHost host, Vector2f minimalSize)
{
    public AUIBox? Parent;

    protected readonly UIHost Host = host;
    
    private Vector2f _minimalSize = minimalSize;

    public Vector2f MinimalSize
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

    private FloatRect _rect;
    public FloatRect Rect
    {
        get => _rect; 
        set
        {
            _rect = new FloatRect(
                value.Left, value.Top,
                float.Max(MinimalSize.X, value.Width),
                float.Max(MinimalSize.Y, value.Height)
            );
            
            Host.UpdateActionsQueue.Enqueue(UpdateLayout);
        }
    } 

    public abstract void UpdateLayout();

    public abstract void Draw(RenderTarget target);

    private readonly List<ClickArea> _areas = [];
    protected ClickAreaView? ClickView;
    public void SetClickView(ClickAreaView? view)
    {
        ClickView = view;
        foreach (var clickArea in _areas)
            clickArea.View = ClickView;
        
        OnClickViewUpdate();
    }

    protected virtual void OnClickViewUpdate()
    {
        
    }

    protected void AddArea(ClickArea area)
    {
        area.View = ClickView;
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