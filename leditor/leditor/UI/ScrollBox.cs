using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace leditor.UI;

class Scroller
{
    public RectangleShape Shape;

    public ClickArea Area;
    public FloatRect Limits;
    public Action<Vector2f> OnUpdate;

    public Scroller(Action<Vector2f> onUpdate, FloatRect limits, Vector2f size, Color color)
    {
        Shape = new RectangleShape
        {
            FillColor = color,
            Size = size
        };
        Area = new ClickArea(new FloatRect(limits.Left, limits.Top, size.X, size.Y))
        {
            OnMove = OnMove
        };
        Limits = limits;
        OnUpdate = onUpdate;
    }
    
    private void OnMove(Vector2f oldPos, Vector2f newPos)
    {
        var pos = new Vector2f(
            float.Clamp(newPos.X - Area.Rect.Width / 2, Limits.Left, Limits.Left + Limits.Width),
            float.Clamp(newPos.Y - Area.Rect.Height / 2, Limits.Top, Limits.Top + Limits.Height)
        );

        Shape.Position = pos;
        Area.Rect.Left = pos.X;
        Area.Rect.Top = pos.Y;

        if (Limits.Width != 0)
        {
            pos.X -= Limits.Left;
            pos.X /= Limits.Width;
        }
        else pos.X = 0;
        
        if (Limits.Height != 0)
        {
            pos.Y -= Limits.Top;
            pos.Y /= Limits.Height;
        }
        else pos.Y = 0;

        OnUpdate(pos);
    }

    public void SetPosition(Vector2f pos)
    {
        Shape.Position = pos;
        Area.Rect.Left = pos.X;
        Area.Rect.Top = pos.Y;
    }
    
    public void SetSize(Vector2f size)
    {
        Shape.Size = size;
        Area.Rect.Width = size.X;
        Area.Rect.Height = size.Y;
    }
}

public class ScrollBox : AUIBox
{
    private AUIElement? _child;
    private View _view = new();

    private Vector2f _difference;
    private Vector2f _scroll;

    private Scroller _scrollerY;
    private Scroller _scrollerX;

    private ClickAreaView _clickView = new(new FloatRect());

    public ScrollBox(UIHost host, AUIElement? child) : base(host, new Vector2f(host.Style.ScrollerThickness * 2, host.Style.ScrollerThickness * 2))
    {
        _child = child;
        if (_child != null)
            _child.SetClickView(_clickView);

        _scrollerX = new Scroller(OnScrollX, new FloatRect(), new Vector2f(host.Style.ScrollerThickness, host.Style.ScrollerThickness), host.Style.ScrollerColor);
        AddArea(_scrollerX.Area);
        
        _scrollerY = new Scroller(OnScrollY, new FloatRect(), new Vector2f(host.Style.ScrollerThickness, host.Style.ScrollerThickness), host.Style.ScrollerColor);
        AddArea(_scrollerY.Area);
    }

    protected override void OnClickViewUpdate()
    {
        throw new InvalidOperationException();
    }

    private void OnScrollX(Vector2f vec)
    {
        SetScroll(new Vector2f(vec.X, _scroll.Y));
    }
    
    private void OnScrollY(Vector2f vec)
    {
        SetScroll(new Vector2f(_scroll.X, vec.Y));
    }

    private void SetScroll(Vector2f scroll)
    {
        _scroll = new Vector2f(
            float.Clamp(scroll.X, 0, 1),
            float.Clamp(scroll.Y, 0, 1)
        );

        if (Child != null)
            Child.Rect = new FloatRect (
            Rect.Left - _scroll.X * _difference.X , 
            Rect.Top - _scroll.Y * _difference.Y,
            Rect.Width - Host.Style.ScrollerThickness,
            Rect.Height - Host.Style.ScrollerThickness
        );
    }
    
    public AUIElement? Child
    {
        get => _child;
        set
        {
            _child = value;
            if (_child == null) return;
            
            _child.SetClickView(_clickView);
            _child.Rect = Rect;
        }
    }

    public override IEnumerable<AUIElement> GetChildren()
        => _child != null ? [_child] : [];

    public override void RemoveChild(AUIElement child)
    {
        if (_child == child)
            _child = null;
    }

    protected override void UpdateMinimalSize() {}
    
    
    public override void UpdateLayout()
    {
        if (_child == null) return;

        var size = new Vector2f(Rect.Width - Host.Style.ScrollerThickness, Rect.Height - Host.Style.ScrollerThickness);
        _child.Rect = new FloatRect (
            Rect.Left - _scroll.X * _difference.X, 
            Rect.Top - _scroll.Y * _difference.Y,
            size.X, size.Y
        );
        
        _difference = new Vector2f(
            float.Max(0, _child.Rect.Width - size.X),
            float.Max(0, _child.Rect.Height - size.Y)
        );

        _clickView.Rect = Rect;
        
        _view.Size = Rect.Size;
        _view.Center = Rect.Position + Rect.Size / 2;
        _view.Viewport = new FloatRect(
            Rect.Left / Host.Size.X,
            
            Rect.Top / Host.Size.Y,
            Rect.Width / Host.Size.X,
            Rect.Height / Host.Size.Y
        );

        var inner = Rect.Size - new Vector2f(Host.Style.ScrollerThickness, Host.Style.ScrollerThickness);

        var len = float.Min(1, inner.Y / _child.Rect.Height) * inner.Y;
        var limits = new FloatRect(
            Rect.Left + Rect.Width - Host.Style.ScrollerThickness,
            Rect.Top,
            0,
            inner.Y - len
        );
        _scrollerY.SetPosition(new Vector2f(limits.Left, limits.Top + _scroll.Y * limits.Height));
        _scrollerY.SetSize(new Vector2f(Host.Style.ScrollerThickness, len));
        _scrollerY.Limits = limits;
        
        len = float.Min(1, inner.X / _child.Rect.Width) * inner.X;
        limits = new FloatRect(
            Rect.Left,
            Rect.Top + Rect.Height - Host.Style.ScrollerThickness,
            inner.X - len, 
            0
        );
        _scrollerX.SetPosition(new Vector2f(limits.Left + _scroll.X * limits.Width, limits.Top));
        _scrollerX.SetSize(new Vector2f(len, Host.Style.ScrollerThickness));
        _scrollerX.Limits = limits;
    }

    private void FinishDraw(RenderTarget target)
    {
        target.Draw(_scrollerX.Shape);
        target.Draw(_scrollerY.Shape);
        target.SetView(Host.View);
    }
    
    public override void Draw(RenderTarget target)
    {
        if (_child == null) return;
        
        target.SetView(_view);
        
        Host.DrawStack.Push(FinishDraw);
        Host.DrawStack.Push(_child.Draw);
    }
}