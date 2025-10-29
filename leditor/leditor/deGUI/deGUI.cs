using Raylib_cs;

namespace leditor.root;

public enum Anchor
{
    Center,
    
    CenterTop,
    CenterBottom,
    RightCenter,
    LeftCenter,
    
    LeftTop,
    LeftBottom,
    RightTop,
    RightBottom,
}

public static class deGUI
{
    public const int ORIGINAL_WIDTH = 960;
    public const int ORIGINAL_HEIGHT = 540;

    public static int ScreenWidth { get; private set; } = ORIGINAL_WIDTH;
    public static int ScreenHeight { get; private set; } = ORIGINAL_HEIGHT;

    public static readonly AreaManager Areas = new();
    public static readonly ButtonManager ButtonManager = new();
    
    public static float WCF;
    public static float HCF;

    public static void Draw()
    {
        WCF = (float)Raylib.GetScreenWidth() / ORIGINAL_WIDTH;
        HCF = (float)Raylib.GetScreenHeight() / ORIGINAL_HEIGHT;

        ScreenWidth = Raylib.GetScreenWidth();
        ScreenHeight = Raylib.GetScreenHeight();
        
        if (MathF.Abs(1 - WCF) > MathF.Abs(1 - HCF)) HCF = WCF;
        else WCF = HCF;
        
        Areas.Process();
        ButtonManager.DrawButtons();
        ButtonManager.ProcessInteractions();
        
        Areas.DrawDebugLayout();
    }

    
    
}