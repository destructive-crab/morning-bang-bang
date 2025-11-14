using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public enum PreserveSide
{
    LeftUp,
    RightDown
}

public class SplitBox: AUIBox
{
    private static Vector2f GetMinimalSize(UIStyle style, UIAxis axis, AUIElement? first, AUIElement? second)
    {
        if (axis == UIAxis.Horizontal)
            return new Vector2f(
                (first?.MinimalSize.X ?? 0) + (second?.MinimalSize.X ?? 0) + style.SplitSeparatorThickness,
                float.Max(first?.MinimalSize.Y ?? 0, second?.MinimalSize.Y ?? 0)
            );
        
        return new Vector2f(
            float.Max(first?.MinimalSize.X ?? 0, second?.MinimalSize.X ?? 0),
            (first?.MinimalSize.Y ?? 0) + (second?.MinimalSize.Y ?? 0) + style.SplitSeparatorThickness
        );
    }

    private float _distance;
    private float _axisSize;
    private readonly ClickArea _area;

    private readonly UIAxis _axis;
    private readonly PreserveSide _preserve;

    private AUIElement? _first;
    private AUIElement? _second;

    private RectangleShape _separator;
    
    public SplitBox(UIHost host, UIAxis axis, AUIElement? first, AUIElement? second, PreserveSide preserve = PreserveSide.LeftUp):
        base(host, GetMinimalSize(host.Style, axis, first, second))
    {
        _preserve = preserve;
        _axis = axis;
        _first = first;
        _second = second;
        
        if (axis == UIAxis.Horizontal)
            _distance = first?.MinimalSize.X ?? 0;
        else
            _distance = first?.MinimalSize.Y ?? 0;

        _axisSize = _distance + Host.Style.SplitSeparatorThickness;

        _area = new ClickArea(
            new FloatRect(
                new Vector2f(), 
                host.Style.SplitSeparatorThickness * (axis == UIAxis.Horizontal ? new Vector2f(1, 0) : new Vector2f(0, 1))
            ), false);
        _area.OnMove = OnMove;
        
        _separator = new RectangleShape(_area.Rect.Size);
        _separator.Position = _area.Rect.Position;
        _separator.FillColor = Host.Style.SplitSeparatorColor;
        
        AddArea(_area);
    }

    private void OnMove(Vector2f oldPosition, Vector2f newPosition)
    {
        if (_axis == UIAxis.Horizontal)
            _distance = newPosition.X - Rect.Left;
        else
            _distance = newPosition.Y - Rect.Top;

        _distance -= (float)Host.Style.SplitSeparatorThickness / 2;
        
        UpdateLayout();
    }

    protected override void UpdateMinimalSize()
        => MinimalSize = GetMinimalSize(Host.Style, _axis, _first, _second);
    
    public override IEnumerable<AUIElement> GetChildren()
        => new[] { _first, _second }
            .OfType<AUIElement>();

    public override void RemoveChild(AUIElement child)
    {
        if (_first == child)
            _first = null;
        else if (_second == child)
            _second = null;
    }

    public override void UpdateLayout()
    {
        if (_axis == UIAxis.Horizontal)
        {
            if (_preserve == PreserveSide.RightDown)
                _distance += Rect.Width - _axisSize;
         
            _distance = float.Max(
                _first?.MinimalSize.X ?? 0,
                float.Min(_distance, Rect.Width - (_second?.MinimalSize.X ?? 0) - Host.Style.SplitSeparatorThickness)
            );
            
            if (_first != null)
                _first.Rect = new FloatRect(Rect.Position.X, Rect.Position.Y, _distance, Rect.Height);
            
            if (_second != null)
                _second.Rect = new FloatRect(
                    Rect.Left + _distance + Host.Style.SplitSeparatorThickness, Rect.Top, 
                    Rect.Width - _distance - Host.Style.SplitSeparatorThickness, Rect.Height
                );
            
            _area.Rect.Left = Rect.Left + _distance;
            _area.Rect.Top = Rect.Top;
            _area.Rect.Height = Rect.Height;
            _axisSize = Rect.Width;
        } else {
            if (_preserve == PreserveSide.RightDown)
                _distance += Rect.Height - _axisSize;
            
            _distance = float.Max(
                _first?.MinimalSize.Y ?? 0,
                float.Min(_distance, Rect.Height - (_second?.MinimalSize.Y ?? 0) - Host.Style.SplitSeparatorThickness)
            );

            if (_first != null)
                _first.Rect = new FloatRect(Rect.Position.X, Rect.Position.Y, Rect.Width, _distance);
            
            if (_second != null)
                _second.Rect = new FloatRect(
                    Rect.Left, Rect.Top + _distance + Host.Style.SplitSeparatorThickness, 
                    Rect.Width, Rect.Height - _distance - Host.Style.SplitSeparatorThickness
                );
            
            _area.Rect.Left = Rect.Left;
            _area.Rect.Top = Rect.Top + _distance;
            _area.Rect.Width = Rect.Width;
            _axisSize = Rect.Height;
        }
        
        _separator.Position = _area.Rect.Position;
        _separator.Size = _area.Rect.Size;
    }

    public override void Draw(RenderTarget target)
    {
        target.Draw(_separator);

        if (_second != null)
            Host.DrawStack.Push(_second.Draw);
        
        if (_first != null)
            Host.DrawStack.Push(_first.Draw);
    }
}