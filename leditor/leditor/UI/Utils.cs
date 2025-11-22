using SFML.Graphics;

namespace leditor.UI;

public static class Utils
{
    public static void CopyView(View src, View dst)
    {
        dst.Size = src.Size;
        dst.Center = src.Center;
        dst.Viewport = src.Viewport;
    }
}