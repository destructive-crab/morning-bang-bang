using deGUISpace;
using Raylib_cs;

namespace leditor.root
{
    public sealed class LeditorStartpoint
    {
        public static void Main()
        { 
            Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.MaximizedWindow);     
            Raylib.InitWindow(deGUI.ORIGINAL_WIDTH, deGUI.ORIGINAL_HEIGHT, "leditor");
            Raylib.SetWindowState(ConfigFlags.MinimizedWindow);
            Raylib.SetWindowState(ConfigFlags.MaximizedWindow);
            
            deGUI.ORIGINAL_WIDTH = Raylib.GetScreenWidth();
            deGUI.ORIGINAL_HEIGHT = Raylib.GetScreenHeight();

            Leditor leditor = new Leditor();
           
            Raylib.SetTargetFPS(60);
            leditor.DoLoop();
        }
    }
}