using System.Numerics;
using SFML.Graphics;

namespace leditor.root
{
    public static class App
    {
        public static Font GeneralFont { get; private set; }
        
        public static InputsHandler InputsHandler => WindowHandler.InputsHandler;
        public static Leditor LeditorInstance { get; private set; }
        public static WindowHandler WindowHandler { get; private set; }

        public static string GeneralPath => "C:\\Users\\destructive_crab\\dev\\band-bang\\leditor\\leditor";

        public static void Main()
        {
            GeneralFont = new Font(GeneralPath + "\\assets\\Autistic.ttf");
            
            WindowHandler = new WindowHandler();
            WindowHandler.CreateWindow();
            
            LeditorInstance = new Leditor();

            LeditorInstance.DoLoop();
        }
        
        private static ProjectData InitTestProject()
        {
            ProjectData project = new ProjectData();
            GridBuffer buffer = new GridBuffer("_");
                    

            
            return project;
        }

        public static void Quit()
        {
        }
    }
}