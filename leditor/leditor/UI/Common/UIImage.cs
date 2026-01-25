using SFML.Graphics;
using SFML.System;
namespace leditor.UI;

public class UIImage : AUIElement
{
    public UIImage(UIHost host, Texture image, IntRect? source = null, Vector2i scaleInPixels = default, Color? color = null) : base(host, new Vector2f(source?.Size.X ?? (int)image.Size.X, source?.Size.Y ?? (int)image.Size.Y))
    {
        ScaleInPixels = scaleInPixels;
        _sprite = new()
        {
            Texture = image,
        };
        Source = source.Value;

        UpdateLayoutP();
    }

    private readonly Sprite _sprite;

    public Texture Image
    {
        get => _sprite.Texture;
        set {
            _sprite.Texture = value;
            MinimalSize = new Vector2f(ScaleInPixels.X, ScaleInPixels.Y);
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
            MinimalSize = new Vector2f(ScaleInPixels.X, ScaleInPixels.Y);
        }
    }

    public Vector2i ScaleInPixels;

    protected override void UpdateLayout()
    {
        _sprite.Position = Rect.Position;

        if (Source != null)
        {
            _sprite.Scale = new Vector2f((float)ScaleInPixels.X / Source.Width, (float)ScaleInPixels.Y / Source.Height);
        }
        else
        {
            _sprite.Scale = new Vector2f(ScaleInPixels.X / (float)Image.Size.X, ScaleInPixels.Y / (float)Image.Size.Y);
        }
    }

    public override void Draw(RenderTarget target)
        => target.Draw(_sprite);
}