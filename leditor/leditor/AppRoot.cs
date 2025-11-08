using System.Numerics;
using deGUISpace;
using Raylib_cs;

namespace leditor.root
{
    public static class App
    {
        public static Leditor LeditorInstance { get; private set; }
        public static WindowHandler WindowHandler { get; private set; }
        
        public static void Main()
        {
            WindowHandler = new WindowHandler();
            WindowHandler.CreateWindow();
            
            LeditorInstance = new Leditor(InitTestProject());
           
            Raylib.SetTargetFPS(60);
            LeditorInstance.Initialize();
            LeditorInstance.DoLoop();
        }
        
        
        private static ProjectData InitTestProject()
        {
            var project = new ProjectData();
            var buffer = new GridBuffer();
                    
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
            
        //anchor test
        // deGUI.ButtonManager.Push(Anchor.RightBottom, -100, -40, 160, 40, "RightBottom", LC, RC);
        // deGUI.ButtonManager.Push(Anchor.RightTop, -100, 40, 160, 40, "RIGHT TOP", LC, RC);
        // deGUI.ButtonManager.Push(Anchor.LeftTop, 100, 40, 160, 40, "TOP LEFT", LC, RC);
        // deGUI.ButtonManager.Push(Anchor.LeftBottom, 100, -40, 160, 40, "BOTTOM LEFT", LC, RC);
    
        // deGUI.ButtonManager.Push(Anchor.LeftCenter, 100, 0, 160, 40, "LEFT CENTER", LC, RC);
        // deGUI.ButtonManager.Push(Anchor.RightCenter, -100, 0, 160, 40, "RIGHT CENTER", LC, RC);
        // deGUI.ButtonManager.Push(Anchor.CenterBottom, 0, -40, 160, 40, "BOTTOM CENTER", LC, RC);
        // deGUI.ButtonManager.Push(Anchor.CenterTop, 0, 40, 160, 40, "TOP CENTER", LC, RC);
           
            var r1 = new GUIRectangle(new Color(33, 33, 38, 90), 5, Color.Black);
            r1.GUIArea = new RectGUIArea(Anchor.LeftTop, 0, 0, 100, deGUI.STRETCH);
    
            deGUI.PushGUIElement(r1);
            var b1 = deGUI.PushButton(Anchor.RightBottom, 0, -10, deGUI.STRETCH, 30, "MENU ITEM 1", LC, RC);
            var b2 = deGUI.PushButton(Anchor.LeftTop, 0, 10, deGUI.STRETCH, 30, "MENU ITEM 2", LC, RC);
            var b3 = deGUI.PushButton(Anchor.LeftTop, 0, 50, -1, 30, "MENU ITEM 3", LC, RC);
    
            b1.SetParent(r1);
            b2.SetParent(r1);
            b3.SetParent(r1);
            
            group = new GUIGroup(new RectGUIArea(Anchor.LeftTop, 0, 0, 500, 300), r1);
            
            group.Hide();
            deGUI.PushGUIElement(group);
            
            var r2 = new GUIRectangle(new Color(0, 255, 0, 100), 5, Color.Black);
            r2.GUIArea = new RectGUIArea(Anchor.Center, 0, 0, 200, -1);
            r2.Show();
            
            var b4 = deGUI.PushButton(Anchor.LeftTop, 0, 0, 50, 30, "1", LC, RC);
            var b5 = deGUI.PushButton(Anchor.RightTop, 0, 0, 50, 30, "2", LC, RC);
            var b6 = deGUI.PushButton(Anchor.LeftBottom, 0, 0, 50, 30, "3", LC, RC);
            var b7 = deGUI.PushButton(Anchor.RightBottom, 0, 0, 50, 30, "4", LC, RC);
            
            r2.AddChild(b4);
            r2.AddChild(b5);
            r2.AddChild(b6);
            r2.AddChild(b7);
            
            deGUI.PushGUIElement(r2);
            
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