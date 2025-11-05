using Raylib_cs;

namespace leditor.root
{
    public static class LeditorRoot
    {
        public static void Main()
        {
            Logger.MinimumLevel = Logger.Level.Debug;
            RaylibLogging.Setup();
            
            Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);     
            Raylib.InitWindow(1080, 720, "leditor");

            Leditor leditor = new Leditor();
           
            Raylib.SetTargetFPS(60);
            leditor.DoLoop();
        }
    }
}