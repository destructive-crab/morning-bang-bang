using Raylib_cs;

namespace leditor.root;

public static class Extensions
{
    public static bool Fits(this Rectangle rect, float x, float y)
    {
        float minx = rect.X - rect.Width / 2;
        float maxx = rect.X + rect.Width / 2;
        
        float miny = rect.Y - rect.Height / 2;
        float maxy = rect.Y + rect.Height / 2;

        if (x > minx && x < maxx)
        {
            if (y > miny && y < maxy)
            {
                return true;
            }
        }

        return false;
    }
    
}