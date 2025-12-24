using System.Numerics;
using System.Reflection;
using leditor.UI;
using SFML.Graphics;
using SFML.System;

namespace leditor.root
{
    public static class App
    {
        public static UIHost UIHost;
        public static InputsHandler InputsHandler => WindowHandler.InputsHandler;
        public static Leditor LeditorInstance { get; private set; }
        public static WindowHandler WindowHandler { get; private set; }

        public static void Main()
        {
            EditorAssets.Initialize();
            
            WindowHandler = new WindowHandler();
            WindowHandler.CreateWindow();
            
            UIHost = new UIHost(new UIStyle(), new Vector2f(App.WindowHandler.Height, App.WindowHandler.Width));
            
            LeditorInstance = new Leditor();

            LeditorInstance.DoLoop();
        }       
        
        public static void Quit()
        {
        }
    }
}