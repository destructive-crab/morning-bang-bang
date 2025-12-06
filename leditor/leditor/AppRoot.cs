using System.Numerics;
using leditor.UI;
using SFML.Graphics;
using SFML.System;

namespace leditor.root
{
    public static class App
    {
        public static UIHost UIHost;
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
            
            UIHost = new UIHost(new UIStyle(), new Vector2f(App.WindowHandler.Height, App.WindowHandler.Width));
            
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