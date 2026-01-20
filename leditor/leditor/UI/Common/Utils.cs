using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public static class Utils
{
    public static void CopyView(View src, View dst)
    {
        dst.Size = src.Size;
        dst.Center = src.Center;
        dst.Viewport = src.Viewport;
    }

    public static Vector2f TextSize(Text text)
    {
        var bounds = text.GetLocalBounds();
        return bounds.Position + bounds.Size;
    }

    public static Vector2f VecI2F(Vector2i orig) => new(orig.X, orig.Y);
    public static Vector2f VecU2F(Vector2u orig) => new(orig.X, orig.Y);
    public static Vector2f ScaleVec(Vector2f orig, Vector2f scale) 
        => new(orig.X * scale.X, orig.Y * scale.Y);

    public static bool PointInRect(FloatRect rect, Vector2f point)
    {
        var inner = point - rect.Position;
        return
            inner.X >= 0 && inner.X <= rect.Width &&
            inner.Y >= 0 && inner.Y <= rect.Height;
    }
}