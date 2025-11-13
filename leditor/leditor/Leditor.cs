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
    private Vector2f pointingOnCell;
    
    //data
    public ProjectData project;
    public GridBuffer buffer = new();
    
    //modules
    private readonly Toolset       Toolset;
    private readonly HotkeysSystem Hotkeys = new();
    private UnitSwitch             UnitSwitch;

    private ProjectHandler projectHandler;

    private float currentZoom = 0;

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
            
            App.WindowHandler.BeginFrame();
            {
                ProcessInputs();
                Hotkeys.Update();
                
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
                }
                App.WindowHandler.CompleteDrawing();   
            }
            App.WindowHandler.CompleteFrame();
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
        if(pointingOnCell.X < buffer.MinX - 1 || pointingOnCell.X > buffer.MaxX) return;
        if(pointingOnCell.Y < buffer.MinY - 1 || pointingOnCell.Y > buffer.MaxY) return;
        
        App.WindowHandler.DrawRectangle((int)(pointingOnCell.X * GridBuffer.CELL_SIZE), (int)(pointingOnCell.Y * GridBuffer.CELL_SIZE),  GridBuffer.CELL_SIZE, GridBuffer.CELL_SIZE, new Color(255, 255, 255, 50));
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

        pointingOnCell = worldPos / GridBuffer.CELL_SIZE;
        
        App.WindowHandler.DrawLine(w/2, 0, w/2, h, Color.Green);
        App.WindowHandler.DrawLine(0, h/2, w, h/2, Color.Green);
        
        //idk why but x needs to be always floored
        if (pointingOnCell.X < 0) pointingOnCell.X = MathF.Floor(pointingOnCell.X);
        else                      pointingOnCell.X = MathF.Floor(pointingOnCell.X);
        
        if (pointingOnCell.Y < 0) pointingOnCell.Y = MathF.Floor(pointingOnCell.Y);
        else                      pointingOnCell.Y = MathF.Floor(pointingOnCell.Y);

        //zoom
        View view = App.WindowHandler.View;

        if (buffer.BufferHeight == 0 || buffer.BufferWidth == 0)
        {
            view.Size = new Vector2f(App.WindowHandler.Width, App.WindowHandler.Height);
        }

        App.WindowHandler.Zoom += App.InputsHandler.MouseWheelDelta / -10;
        
        //move and tools
        if (App.InputsHandler.IsRightMouseButtonPressed)
        {
            Vector2f delta = (Vector2f)(prevMousePos - App.InputsHandler.MousePosition);
            
            delta.X *= (view.Size.X / w);
            delta.Y *= (view.Size.Y / h);
            
            view.Move(delta);
        }
        else if(App.InputsHandler.IsLeftMouseButtonDown)
        {
            Toolset.CurrentTool?.OnClick(new Vector2(pointingOnCell.X, pointingOnCell.Y), buffer);
        }

 //       if (view.Target.X < buffer.WorldMinX - 300) view.Target.X = buffer.WorldMinX - 300;
 //       if (view.Target.X > buffer.WorldMaxX + 300) view.Target.X = buffer.WorldMaxX + 300;
 //       if (view.Target.Y < buffer.WorldMinY - 300) view.Target.Y = buffer.WorldMinY - 300;
 //       if (view.Target.Y > buffer.WorldMaxY + 300) view.Target.Y = buffer.WorldMaxY + 300;

        prevMousePos = App.InputsHandler.MousePosition;
    }

    private Vector2i prevMousePos;

    public void FocusOnBufferCenter()
    {
        App.WindowHandler.View.Center = new Vector2f(
            /* x */ buffer.WorldMinX + buffer.WorldBufferWidth  / 2f,
            /* y */ buffer.WorldMinY + buffer.WorldBufferHeight / 2f);

        float xZoom = App.WindowHandler.Width  / (float) (buffer.WorldBufferWidth  + 240);
        float yZoom = App.WindowHandler.Height / (float) (buffer.WorldBufferHeight + 240);

        float ratio = App.WindowHandler.Width / (float)App.WindowHandler.Height;
            
        if (buffer.BufferWidth > buffer.BufferHeight)
        {
            App.WindowHandler.ViewWidth = buffer.WorldBufferWidth + 340;
            App.WindowHandler.ViewHeight = (int)((buffer.WorldBufferWidth + 340) * (1f/ratio));
        }
        else
        {
            App.WindowHandler.ViewWidth = (int)((buffer.WorldBufferHeight + 340) * ratio);
            App.WindowHandler.ViewHeight = (buffer.WorldBufferHeight + 340);
        }
    }
}