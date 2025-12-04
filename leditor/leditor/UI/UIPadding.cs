namespace leditor.UI;

public struct UIPadding(float left, float right, float top, float bottom)
{
    public float Left = left;
    public float Right = right;
    public float Top = top;
    public float Bottom = bottom;

    public UIPadding() : this(0, 0, 0, 0) { }
}