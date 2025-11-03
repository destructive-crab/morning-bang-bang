using deGUISpace;
using Raylib_cs;
using rlImGui_cs;

namespace leditor.root
{
    public sealed class LeditorRoot
    {
        public static void Main()
        { 
            Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);     
            Raylib.InitWindow(deGUI.ORIGINAL_WIDTH, deGUI.ORIGINAL_HEIGHT, "leditor");
            rlImGui.Setup(true);

            Leditor leditor = new Leditor();
           
            Raylib.SetTargetFPS(60);
            leditor.DoLoop();
        }
    }
}