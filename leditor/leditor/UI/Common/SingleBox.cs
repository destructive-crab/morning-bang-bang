using SFML.Graphics;
namespace leditor.UI;

public class SingleBox : AUIBox
{
    private AUIElement? _child;

    public AUIElement? Child
    {
        get => _child;
        set {
            if (value != null)
                value.Parent = this;
            else if (_child != null)
                _child.Parent = null;
            _child = value;
            UpdateMinimalSize();
        }
    }
    
    private bool _hide;

    public bool Hide
    {
        get => _hide;
        set {
            _hide = value;
            UpdateMinimalSize();
        }
    }

    public SingleBox(UIHost host, AUIElement? child = null, bool hide = false) : base(host, child?.MinimalSize ?? default)
    {
        _hide = hide;
        _child = child;
        if (_child != null)
            _child.Parent = this;
    }
    
    public override IEnumerable<AUIElement> GetChildren()
        => _child == null ? [] : [_child];

    public override void RemoveChild(AUIElement child)
    {
        if (_child == child)
        {
            child.Parent = null;
            Child = null;
        }
    }
    
    public override void UpdateLayout()
    {
        if (_child != null && !_hide)
            _child.Rect = Rect;
    }

    protected override void UpdateMinimalSize()
    {
        if (_hide) return;
        MinimalSize = _child?.MinimalSize ?? default;
    }
    
    public override void Draw(RenderTarget target)
    {
        if (_child != null && !_hide)
        {
            Host.DrawStack.Push(_child.Draw);
        }
    }

    public override void ProcessClicks()
    {
        if (_child != null && !_hide)
            Host.ClickHandlersStack.Push(_child.ProcessClicks);
    }
}