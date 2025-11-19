using System.Numerics;
using deGUISpace;
using Newtonsoft.Json;
using SFML.Graphics;

namespace leditor.root
{
    public static class App
    {
        public static Font GeneralFont { get; private set; }
        
        public static InputsHandler InputsHandler => WindowHandler.InputsHandler;
        public static Leditor LeditorInstance { get; private set; }
        public static WindowHandler WindowHandler { get; private set; }
        
        public static void Main()
        {
            GeneralFont =
                new Font("C:\\Users\\destructive_crab\\dev\\band-bang\\leditor\\leditor\\assets\\Autistic.ttf");
            
            WindowHandler = new WindowHandler();
            WindowHandler.CreateWindow();

            string output = InitTestProject().Export();
            
            LeditorInstance = new Leditor(ProjectData.Import(output));
            
            LeditorInstance.Initialize();
            LeditorInstance.DoLoop();
        }
        
        private static ProjectData InitTestProject()
        {
            ProjectData project = new ProjectData();
            GridBuffer buffer = new GridBuffer();
                    
            TextureData tex = project.AddTexture("red", "C:\\Users\\destructive_crab\\dev\\band-bang\\leditor\\leditor\\assets\\tests\\red.png");
            project.AddTile("red", tex);
    
            project.CreateTilesFromTileset("wall_up",
                "C:\\Users\\destructive_crab\\dev\\band-bang\\leditor\\leditor\\assets\\tests\\wall_up.png");
                    
            buffer.SetTile(new Vector2(3, 0), "wall_up_1");
            buffer.SetTile(new Vector2(4, 0), "wall_up_3");
            buffer.SetTile(new Vector2(5, 0), "wall_up_5");
            buffer.SetTile(new Vector2(3, 1), "wall_up_2");
            buffer.SetTile(new Vector2(4, 1), "wall_up_4");
            buffer.SetTile(new Vector2(5, 1), "wall_up_6");
            
            project.AddMap("map_1", buffer.Get);
    
            buffer.Clear();
            
            buffer.SetTile(new Vector2(3, 0), "wall_up_1");
            buffer.SetTile(new Vector2(4, 1), "wall_up_3");
            buffer.SetTile(new Vector2(5, 2), "wall_up_5");
            buffer.SetTile(new Vector2(3, 3), "wall_up_2");
            buffer.SetTile(new Vector2(4, 4), "wall_up_4");
            buffer.SetTile(new Vector2(5, 5), "wall_up_6");
            
            project.AddMap("map_2", buffer.Get);
            
            buffer.Clear();
            
            buffer.SetTile(new Vector2(1, 0), "wall_up_1");
            buffer.SetTile(new Vector2(1, 1), "wall_up_3");
            buffer.SetTile(new Vector2(1, 2), "wall_up_5");
            buffer.SetTile(new Vector2(1, 3), "wall_up_2");
            buffer.SetTile(new Vector2(1, 4), "wall_up_4");
            buffer.SetTile(new Vector2(1, 5), "wall_up_6");
            
            project.AddMap("map_3", buffer.Get);
            
            project.AddUnit("unit_1", "map_1", UnitData.NO_OVERRIDE);
            project.AddUnit("unit_2", "map_2", UnitData.NO_OVERRIDE);
            project.AddUnit("unit_3", "map_3", UnitData.NO_OVERRIDE);
            
            return project;
        }
    
        private static GUIGroup group;
    
        private static void RC()
        {
            Pr("RIGHT CLICK BUTTON");
        }
    
        private static void LC()
        {
            Pr("LEFT CLICK BUTTON");
        }
        
        private static void Pr(string str)
        {
            Console.WriteLine(str);
        }
    }
}