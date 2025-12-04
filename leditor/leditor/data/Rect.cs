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
    
    public static bool operator== (Rect obj1, Rect obj2)
    {
        return Equals(obj1, obj2);
    }

    // this is second one '!='
    public static bool operator!= (Rect obj1, Rect obj2)
    {
        return !Equals(obj1, obj2);
    } 
    
    public bool Equals(Rect other)
    {
        return StartX == other.StartX && StartY == other.StartY && Width == other.Width && Height == other.Height;
    }

    public override bool Equals(object? obj)
    {
        return obj is Rect other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(StartX, StartY, Width, Height);
    }
}