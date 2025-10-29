using System.Numerics;
using Raylib_cs;
using rlImGui_cs;

namespace leditor.root;

public sealed class Leditor
{
    public ProjectData project;
    public GridBuffer buffer = new();
    private Camera2D camera;

    private Vector2 pointingOn;
    private readonly Toolset Toolset = new();
    
    public void DoLoop()
    {
        camera = new Camera2D(new Vector2(0, 0), new Vector2(550, 450), 0, 0.1f);
        
        while (!Raylib.WindowShouldClose())
        {
            if(project == null)
            {
                InitTestProject();

                continue;
            }

            //inputs
            ProcessInputs();
            
            //drawing
            Raylib.BeginDrawing();
            rlImGui.Begin();
            {
                Raylib.ClearBackground(new Color(19, 38, 35));

                Raylib.BeginMode2D(camera);
                
                buffer.DrawTiles(project);
                DrawGrid();

//                Toolset.DrawToolsetGUI(project);
                
                Raylib.EndMode2D();
                
                deGUI.Draw();
            }
            rlImGui.End();
            Raylib.EndDrawing();
        }
        rlImGui.Shutdown();
    }

    public void DrawGrid()
    {
        int sx = 0;
        int sy = GridBuffer.CELL_SIZE * 30;
        
        int x = sx;
        int y = sy;
        
        int dx = GridBuffer.CELL_SIZE;
        int dy = -GridBuffer.CELL_SIZE;

        int ex = 10000;
        int ey = -10000;
        
        Color color = Color.White;
        color.A = 169;
        
        for (; x < ex; x += dx)
        {
            Raylib.DrawLine(x, sy, x, ey, color);
        }
        for (; y > ey; y += dy) 
        {
            Raylib.DrawLine(sx, y, ex, y, color);
        }
        
        Raylib.DrawRectangle((int)(pointingOn.X * GridBuffer.CELL_SIZE), (int)(pointingOn.Y * GridBuffer.CELL_SIZE),  GridBuffer.CELL_SIZE, GridBuffer.CELL_SIZE, new Color(255, 255, 255, 50));
    }
    
    private void ProcessInputs()
    {
        int w = Raylib.GetScreenWidth();
        int h = Raylib.GetScreenHeight();
        
        Rectangle area = new Rectangle(new Vector2(w/2, h/2), w*0.85f, h*0.8f);
        Vector2 mousePos = Raylib.GetMousePosition();

        var color = Color.Green;
        color.A = 50;
        
        //Raylib.DrawRectanglePro(area, new Vector2(area.Width/2, area.Height/2), 0, color);

        //other
        Vector2 worldPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
        
        pointingOn = worldPos / GridBuffer.CELL_SIZE;
        pointingOn.X = (int)(pointingOn.X);
        pointingOn.Y = (int)(pointingOn.Y);
            
        if (!area.Fits(mousePos.X, mousePos.Y))
        {
            prevMousePos = Raylib.GetMousePosition();
            return;
        }
        
        //zoom
        camera.Zoom = camera.Zoom + 0.05f * Raylib.GetMouseWheelMove();
        
        if (camera.Zoom > 4f) camera.Zoom = 4;
        if (camera.Zoom < 0.2f) camera.Zoom = 0.2f;
        
        //move and tools
        if (Raylib.IsMouseButtonDown(MouseButton.Right))
        {
            Vector2 delta = prevMousePos - Raylib.GetMousePosition();
            camera.Target += delta  / camera.Zoom;
        }
        else if(Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            Toolset.CurrentTool?.OnClick(pointingOn, buffer);
        }
        
        prevMousePos = Raylib.GetMousePosition();
    }

    private Vector2 prevMousePos;

    private void InitTestProject()
    {
        project = new ProjectData();
                
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
       
        //anchor test
       // deGUI.ButtonManager.Push(Anchor.RightBottom, -100, -40, 160, 40, "RightBottom", LC, RC);
       // deGUI.ButtonManager.Push(Anchor.RightTop, -100, 40, 160, 40, "RIGHT TOP", LC, RC);
       // deGUI.ButtonManager.Push(Anchor.LeftTop, 100, 40, 160, 40, "TOP LEFT", LC, RC);
       // deGUI.ButtonManager.Push(Anchor.LeftBottom, 100, -40, 160, 40, "BOTTOM LEFT", LC, RC);
//
       // deGUI.ButtonManager.Push(Anchor.LeftCenter, 100, 0, 160, 40, "LEFT CENTER", LC, RC);
       // deGUI.ButtonManager.Push(Anchor.RightCenter, -100, 0, 160, 40, "RIGHT CENTER", LC, RC);
       // deGUI.ButtonManager.Push(Anchor.CenterBottom, 0, -40, 160, 40, "BOTTOM CENTER", LC, RC);
       // deGUI.ButtonManager.Push(Anchor.CenterTop, 0, 40, 160, 40, "TOP CENTER", LC, RC);
       // 
       // deGUI.ButtonManager.Push(Anchor.Center, 0, 0, 160, 40, "CENTER", LC, RC);
        
    }

    private void RC()
    {
        Pr("RIGHT CLICK BUTTON");
    }

    private void LC()
    {
        Pr("LEFT CLICK BUTTON");
    }
    
    private void Pr(string str)
    {
        Console.WriteLine(str);
    }
}