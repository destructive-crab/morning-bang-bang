using SFML.Graphics;
using SFML.System;
namespace leditor.UI;

public class UIImage(UIHost host, Texture image, IntRect? source = null, Color? color = null) : 
    AUIElement(host, new Vector2f(source?.Size.X ?? (int)image.Size.X, source?.Size.Y ?? (int)image.Size.Y))
{
    private readonly Sprite _sprite = new()
    {
        Texture = image,
        TextureRect = source ?? new IntRect(0, 0, (int)image.Size.X, (int)image.Size.Y)
    };
    
    public Texture Image
    {
        get => _sprite.Texture;
        set {
            _sprite.Texture = value;
            MinimalSize = _sprite.GetLocalBounds().Size;
        }
    }
    
    public Color Color
    {
        get => _sprite.Color;
        set => _sprite.Color = value;
    }

    public IntRect Source
    {
        get => _sprite.TextureRect;
        set
        {
            _sprite.TextureRect = value;
            MinimalSize = _sprite.GetLocalBounds().Size;
        }
    }
    
    public override void UpdateLayout() {}

    public override void Draw(RenderTarget target)
        => target.Draw(_sprite);
}