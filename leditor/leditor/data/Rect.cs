using SFML.Graphics;

namespace leditor.root;

public struct Rect
{
    public int StartX;
    public int StartY;

    public int Width;
    public int Height;

    public Rect(int startX, int startY, int width, int height)
    {
        StartX = startX;
        StartY = startY;
        Width = width;
        Height = height;
    }

    public IntRect ToIntRect()
    {
        return new IntRect(StartX, StartY, Width, Height);
    }
}