using System;
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
        
        UnitSwitch = new UnitSwitch();
    }

    public void Initialize()
    {
        Toolset.BuildGUI();
        UnitSwitch.BuildGUI();
        FocusOnBufferCenter();
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
            DrawGridLayout();

            Raylib.EndMode2D();
            
            deGUI.Draw();
            Hotkeys.Update();
            
            Raylib.EndDrawing();
        }
    }

    public void DrawGridLayout()
    {
        int sx = (buffer.MinX - 1) * GridBuffer.CELL_SIZE;
        int sy = (buffer.MinY - 1) * GridBuffer.CELL_SIZE;
        
        int x = sx;
        int y = sy;
        
        int dx = GridBuffer.CELL_SIZE;
        int dy = GridBuffer.CELL_SIZE;

        int ex = (buffer.MaxX + 1) * GridBuffer.CELL_SIZE;
        int ey = (buffer.MaxY + 1) * GridBuffer.CELL_SIZE;
        
        Color color = Color.White;
        color.A = 169;
        
        for (; x <= ex; x += dx)
        {
            Raylib.DrawLine(x, sy, x, ey, color);
        }
        for (; y <= ey; y += dy) 
        {
            Raylib.DrawLine(sx, y, ex, y, color);
        }

        if (deGUI.HoveringSomething) return;
        if(pointingOn.X < buffer.MinX - 1 || pointingOn.X > buffer.MaxX) return;
        if(pointingOn.Y < buffer.MinY - 1 || pointingOn.Y > buffer.MaxY) return;
        
        Raylib.DrawRectangle((int)(pointingOn.X * GridBuffer.CELL_SIZE), (int)(pointingOn.Y * GridBuffer.CELL_SIZE),  GridBuffer.CELL_SIZE, GridBuffer.CELL_SIZE, new Color(255, 255, 255, 50));
    }

    private void ProcessInputs()
    {
        if (deGUI.HoveringSomething) return;

        int w = Raylib.GetScreenWidth();
        int h = Raylib.GetScreenHeight();

        Rectangle area = new Rectangle(new Vector2(w / 2, h / 2), w * 0.85f, h * 0.8f);
        Vector2 mousePos = Raylib.GetMousePosition();

        //other
        Vector2 worldPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
        camera.Offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);

        pointingOn = worldPos / GridBuffer.CELL_SIZE;
        
        Raylib.DrawLine(w/2, 0, w/2, h, Color.Green);
        Raylib.DrawLine(0, h/2, w, h/2, Color.Green);
        
        //idk why but x needs to be always floored
        if (pointingOn.X < 0) pointingOn.X = MathF.Floor(pointingOn.X);
        else                  pointingOn.X = MathF.Floor(pointingOn.X);
        
        if (pointingOn.Y < 0) pointingOn.Y = MathF.Floor(pointingOn.Y);
        else                  pointingOn.Y = MathF.Floor(pointingOn.Y);

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
            camera.Target += delta / camera.Zoom;
        }
        else if(Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            Toolset.CurrentTool?.OnClick(pointingOn, buffer);
        }

        if (camera.Target.X < buffer.WorldMinX - 300) camera.Target.X = buffer.WorldMinX - 300;
        if (camera.Target.X > buffer.WorldMaxX + 300) camera.Target.X = buffer.WorldMaxX + 300;
        if (camera.Target.Y < buffer.WorldMinY - 300) camera.Target.Y = buffer.WorldMinY - 300;
        if (camera.Target.Y > buffer.WorldMaxY + 300) camera.Target.Y = buffer.WorldMaxY + 300;
        
        prevMousePos = Raylib.GetMousePosition();
    }

    private Vector2 prevMousePos;

    public void FocusOnBufferCenter()
    {
        camera.Target = new Vector2(
            /* x */ buffer.WorldMinX + buffer.WorldBufferWidth  / 2f,
            /* y */ buffer.WorldMinY + buffer.WorldBufferHeight / 2f);

        float xZoom = Raylib.GetScreenWidth()  / (float) (buffer.WorldBufferWidth  + 240);
        float yZoom = Raylib.GetScreenHeight() / (float) (buffer.WorldBufferHeight + 240);

        Console.WriteLine(buffer.WorldBufferWidth + " " + buffer.WorldBufferHeight +" "+ xZoom + " " + yZoom);
        
        if (xZoom < yZoom) camera.Zoom = xZoom;
        else               camera.Zoom = yZoom;
    }
}