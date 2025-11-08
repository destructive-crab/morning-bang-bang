using System.Numerics;
using Raylib_cs;

namespace leditor.UI;

public class UIImage(UIHost host, Texture2D image, Rectangle? source = null, Color? color = null) : 
    AUIElement(host, source?.Size ?? new Vector2(image.Width, image.Height))
{
    public Texture2D Image = image;
    public Color Color = color ?? Color.White;


    private Rectangle _source = source ?? new Rectangle(Vector2.Zero, image.Width, image.Height);

    public Rectangle Source
    {
        get => _source;
        set
        {
            _source = value;
            MinimalSize = _source.Size;
        }
    }
    
    public override void UpdateLayout() {}

    public override void Draw()
        => Raylib.DrawTextureRec(Image, _source, Rect.Position, Color);
}