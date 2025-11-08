using System.Numerics;
using deGUISpace;
using Raylib_cs;

namespace leditor.root;

public sealed class Leditor
{
    //data
    public ProjectData project;
    public GridBuffer buffer = new();
    private Camera2D camera;

    private Vector2 pointingOn;
    
    //modules

    private readonly Toolset       Toolset;
    private readonly HotkeysSystem Hotkeys = new();
    private UnitSwitch             UnitSwitch;

    private ProjectHandler projectHandler;

    public Leditor(ProjectData project)
    {
        this.project = project;
        
        Toolset = new Toolset();
        
        UnitSwitch = new UnitSwitch(this);
    }

    public void Initialize()
    {
        Toolset.BuildGUI();
        UnitSwitch.BuildGUI();
    }

    public void DoLoop()
    {
        camera = new Camera2D(new Vector2(0, 0), new Vector2(550, 450), 0, 0.1f);
        
        while (!Raylib.WindowShouldClose())
        {
            //inputs
            ProcessInputs();
            
            //drawing
            Raylib.BeginDrawing();

            Raylib.ClearBackground(new Color(19, 38, 35));

            Raylib.BeginMode2D(camera);
            
            buffer.DrawTiles(project);
            DrawGrid();

            Raylib.EndMode2D();
            
            deGUI.Draw();
            Hotkeys.Update();
            
            Raylib.EndDrawing();
        }
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
        
        if(deGUI.HoveringSomething) return;
        Raylib.DrawRectangle((int)(pointingOn.X * GridBuffer.CELL_SIZE), (int)(pointingOn.Y * GridBuffer.CELL_SIZE),  GridBuffer.CELL_SIZE, GridBuffer.CELL_SIZE, new Color(255, 255, 255, 50));
    }

    private void ProcessInputs()
    {
        if(deGUI.HoveringSomething) return;
        
        int w = Raylib.GetScreenWidth();
        int h = Raylib.GetScreenHeight();
        
        Rectangle area = new Rectangle(new Vector2(w/2, h/2), w*0.85f, h*0.8f);
        Vector2 mousePos = Raylib.GetMousePosition();

        var color = Color.Green;
        color.A = 50;
        
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

}