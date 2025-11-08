using System.Numerics;
using leditor.root;
using Raylib_cs;

namespace leditor.UI;

public enum PreserveSide
{
    LeftUp,
    RightDown
}

public class SplitBox: AUIBox
{
    private static Vector2 GetMinimalSize(UIStyle style, UIAxis axis, AUIElement? first, AUIElement? second)
    {
        if (axis == UIAxis.Horizontal)
            return new Vector2(
                (first?.MinimalSize.X ?? 0) + (second?.MinimalSize.X ?? 0) + style.SplitSeparatorThickness,
                float.Max(first?.MinimalSize.Y ?? 0, second?.MinimalSize.Y ?? 0)
            );
        
        return new Vector2(
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
            new Rectangle(
                Vector2.Zero, 
                host.Style.SplitSeparatorThickness * (axis == UIAxis.Horizontal ? Vector2.UnitX : Vector2.UnitY)
            ), false);
        _area.OnMove = OnMove;
        AddArea(_area);
    }

    private void OnMove(Vector2 oldPosition, Vector2 newPosition)
    {
        if (_axis == UIAxis.Horizontal)
            _distance = newPosition.X - Rect.X;
        else
            _distance = newPosition.Y - Rect.Y;

        _distance -= (float)Host.Style.SplitSeparatorThickness / 2;
        
        UpdateLayout();
    }
    
    public override void UpdateMinimalSize()
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
                _first.Rect = new Rectangle(Rect.Position, _distance, Rect.Height);
            
            if (_second != null)
                _second.Rect = new Rectangle(
                    Rect.X + _distance + Host.Style.SplitSeparatorThickness, Rect.Y, 
                    Rect.Width - _distance - Host.Style.SplitSeparatorThickness, Rect.Height
                );
            
            _area.Rect.X = Rect.X + _distance;
            _area.Rect.Y = Rect.Y;
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
                _first.Rect = new Rectangle(Rect.Position, Rect.Width, _distance);
            
            if (_second != null)
                _second.Rect = new Rectangle(
                    Rect.X, Rect.Y + _distance + Host.Style.SplitSeparatorThickness, 
                    Rect.Width, Rect.Height - _distance - Host.Style.SplitSeparatorThickness
                );
            
            _area.Rect.X = Rect.X;
            _area.Rect.Y = Rect.Y + _distance;
            _area.Rect.Width = Rect.Width;
            _axisSize = Rect.Height;
        }
    }

    public override void Draw()
    {
        Raylib.DrawRectangleRec(_area.Rect, Host.Style.SplitSeparatorColor);

        if (_second != null)
            Host.DrawStack.Push(_second.Draw);
        
        if (_first != null)
            Host.DrawStack.Push(_first.Draw);
    }
}