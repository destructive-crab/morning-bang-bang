using deGUISpace;
using Raylib_cs;

namespace leditor.root;

public class WindowHandler
{
    public bool IsWindowCreated { get; private set; } = false;
    
    public void CreateWindow()
    {
        if(IsWindowCreated) return;
        
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.MaximizedWindow);     
        Raylib.InitWindow(deGUI.ORIGINAL_WIDTH, deGUI.ORIGINAL_HEIGHT, "leditor");
        Raylib.SetWindowState(ConfigFlags.MinimizedWindow);
        Raylib.SetWindowState(ConfigFlags.MaximizedWindow);
        
        deGUI.ORIGINAL_WIDTH = Raylib.GetScreenWidth();
        deGUI.ORIGINAL_HEIGHT = Raylib.GetScreenHeight();

        IsWindowCreated = true;
    }
}