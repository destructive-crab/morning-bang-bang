/*using System.Numerics;
using Raylib_cs;
using SFML.Graphics;

namespace leditor.UI;

public class ScrollBox(UIHost host, AUIElement? child) : AUIBox(host, Vector2.Zero)
{
    private AUIElement? _child = child;

    public AUIElement? Child
    {
        get => _child;
        set
        {
            _child = value;
            UpdateLayout();
        }
    }

    public override IEnumerable<AUIElement> GetChildren()
        => _child != null ? [_child] : [];

    public override void RemoveChild(AUIElement child)
    {
        if (_child == child)
            _child = null;
        UpdateLayout();
    }
    
    public override void UpdateMinimalSize() {}

    private RenderTexture2D _renderTexture = Raylib.LoadRenderTexture(
        (int)(child?.MinimalSize.X ?? 0), 
        (int)(child?.MinimalSize.Y ?? 0)
    );
    
    public override void UpdateLayout()
    {
        if (_child == null) return;
        
        _child.Rect = new Rectangle(Vector2.Zero, Rect.Size);
        _renderTexture = Raylib.LoadRenderTexture((int)Rect.X, (int)Rect.Y);
    }

    private void BeginDraw()
    {
        Raylib.BeginTextureMode(_renderTexture);
    }
    
    private void EndDraw()
    {
        Raylib.EndTextureMode();
        Raylib.DrawTextureV(_renderTexture.Texture, Rect.Position, Color.White);
    }
    
    public override void Draw(RenderTarget target)
    {
        if (_child == null) return;

        BeginDraw();
        Host.DrawStack.Push(EndDraw);
        Host.DrawStack.Push(_child.Draw);
    }
}*/