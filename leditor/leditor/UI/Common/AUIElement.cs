using SFML.Graphics;
using SFML.System;
using SFML.Window;

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
            
            if (Parent != null)
            {
                Host.UpdateActionsQueue.Enqueue(Parent.OnChildUpdate);
            }
        }
    }

    private FloatRect _rect;
    public FloatRect Rect
    {
        get => _rect; 
        set
        {
            if (_rect == value) return;
            
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

    public virtual void ProcessClicks() {}
    
    public void Destroy()
    {
        Parent?.RemoveChild(this);
        Parent = null;
    }

    public virtual void Deactivate() {}
    public virtual void OnTextEntered(string text) { }
    public virtual void OnKeyPressed(Keyboard.Key key) {}
    
    public virtual void OnMouseClick(Vector2f pos) {}

}