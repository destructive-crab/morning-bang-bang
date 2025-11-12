using System.Drawing;
using System.Numerics;
using deGUISpace;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Color = SFML.Graphics.Color;

namespace leditor.root;

public sealed class Leditor
{
    //data
    public ProjectData project;
    public GridBuffer buffer = new();

    private Vector2f pointingOn;
    
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
        while (App.WindowHandler.IsOpen)
        {
            //inputs
            ProcessInputs();
            
            //drawing
            App.WindowHandler.BeginDrawing();
            {
                buffer.DrawTiles(project);
                DrawGridLayout();
                
                App.WindowHandler.BeginGUIMode();
                {
                    deGUI.Draw();
                }
                App.WindowHandler.CompleteGUIMode();
                
                Hotkeys.Update();

            }
            App.WindowHandler.CompleteDrawing();
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
            App.WindowHandler.DrawLine(x, sy, x, ey, color);
        }
        for (; y <= ey; y += dy) 
        {
            App.WindowHandler.DrawLine(sx, y, ex, y, color);
        }

        if (deGUI.HoveringSomething) return;
        if(pointingOn.X < buffer.MinX - 1 || pointingOn.X > buffer.MaxX) return;
        if(pointingOn.Y < buffer.MinY - 1 || pointingOn.Y > buffer.MaxY) return;
        
        App.WindowHandler.DrawRectangle((int)(pointingOn.X * GridBuffer.CELL_SIZE), (int)(pointingOn.Y * GridBuffer.CELL_SIZE),  GridBuffer.CELL_SIZE, GridBuffer.CELL_SIZE, new Color(255, 255, 255, 50));
    }

    private void ProcessInputs()
    {
        if (deGUI.HoveringSomething) return;

        int w = App.WindowHandler.Width;
        int h = App.WindowHandler.Height;

        Rectangle area = new Rectangle(w / 2, h / 2, (int)(w * 0.85f), (int)(h * 0.8f));
        Vector2i mousePos = Mouse.GetPosition();

        //other
        Vector2f worldPos = App.InputsHandler.WorldMousePosition;

        pointingOn = worldPos / GridBuffer.CELL_SIZE;
        
        App.WindowHandler.DrawLine(w/2, 0, w/2, h, Color.Green);
        App.WindowHandler.DrawLine(0, h/2, w, h/2, Color.Green);
        
        //idk why but x needs to be always floored
        if (pointingOn.X < 0) pointingOn.X = MathF.Floor(pointingOn.X);
        else                  pointingOn.X = MathF.Floor(pointingOn.X);
        
        if (pointingOn.Y < 0) pointingOn.Y = MathF.Floor(pointingOn.Y);
        else                  pointingOn.Y = MathF.Floor(pointingOn.Y);

        //zoom
        View view = App.WindowHandler.View;
        //view.Zoom(0.05f * App.InputsHandler.MouseWheelDelta);

        //if (view.Size.X < 100) view.Size = new Vector2f(100, view.Size.Y);
        //if (view.Size.X > 800) view.Size = new Vector2f(800, view.Size.Y);
        
        //move and tools
        if (App.InputsHandler.IsRightMouseButtonPressed)
        {
            Vector2i delta = prevMousePos - Mouse.GetPosition();
            view.Move((Vector2f)delta);
        }
        else if(App.InputsHandler.IsLeftMouseButtonDown)
        {
            Toolset.CurrentTool?.OnClick(new Vector2(pointingOn.X, pointingOn.Y), buffer);
        }

 //       if (view.Target.X < buffer.WorldMinX - 300) view.Target.X = buffer.WorldMinX - 300;
 //       if (view.Target.X > buffer.WorldMaxX + 300) view.Target.X = buffer.WorldMaxX + 300;
 //       if (view.Target.Y < buffer.WorldMinY - 300) view.Target.Y = buffer.WorldMinY - 300;
 //       if (view.Target.Y > buffer.WorldMaxY + 300) view.Target.Y = buffer.WorldMaxY + 300;

         prevMousePos = Mouse.GetPosition();
    }

    private Vector2i prevMousePos;

    public void FocusOnBufferCenter()
    {
        App.WindowHandler.View.Center = new Vector2f(
            /* x */ buffer.WorldMinX + buffer.WorldBufferWidth  / 2f,
            /* y */ buffer.WorldMinY + buffer.WorldBufferHeight / 2f);

        float xZoom = App.WindowHandler.Width  / (float) (buffer.WorldBufferWidth  + 240);
        float yZoom = App.WindowHandler.Height / (float) (buffer.WorldBufferHeight + 240);

        Console.WriteLine(buffer.WorldBufferWidth + " " + buffer.WorldBufferHeight +" "+ xZoom + " " + yZoom);
        
//        if (xZoom < yZoom) view.Zoom = xZoom;
//        else               view.Zoom = yZoom;
    }
}