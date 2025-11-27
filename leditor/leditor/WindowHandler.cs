using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Color = SFML.Graphics.Color;

namespace leditor.root;

public class WindowHandler
{
    public int ViewWidth;
    public int ViewHeight;

    public float Zoom = 1;
    
    public View View { get; private set; }
    public InputsHandler InputsHandler { get; private set; }
    
    public bool IsWindowCreated { get; private set; } = false;
    public bool IsOpen => window.IsOpen;
    public int Width => (int)window.Size.X;
    public int Height => (int)window.Size.Y;

    public static int ORIGINAL_WIDTH = 1280;
    public static int ORIGINAL_HEIGHT = 1040;

    public RenderWindow window;
    
    public void CreateWindow()
    {
        if(IsWindowCreated) return;

        window = new RenderWindow(VideoMode.DesktopMode, "MORNING THRILLER LEVEL EDITOR", Styles.Default);

        InputsHandler = new InputsHandler(window);
        
        window.SetFramerateLimit(60);
        View = new View(new Vector2f(0, 0), new Vector2f(Width, Height));
        window.SetView(View);
        
        IsWindowCreated = true;
    }

    public void BeginGUIMode()
    {
        window.SetView(window.DefaultView);
    }
    
    public void CompleteGUIMode()
    {
        View.Size = new Vector2f(ViewWidth * Zoom, ViewHeight * Zoom);
        
        window.SetView(View);
    }
    
    public void DrawLine(int startX, int startY, int endX, int endY, Color color)
    {
        VertexArray lines = new VertexArray(PrimitiveType.Lines, 2);
        lines.Append(new Vertex(new Vector2f(startX, startY), color));
        lines.Append(new Vertex(new Vector2f(endX, endY), color));
        
        window.Draw(lines);
    }

    public void DrawRectangle(int startX, int startY, int width, int height, Color color)
    {
        RectangleShape rect = new RectangleShape(new Vector2f(width, height));
        rect.FillColor = color;
        rect.Position = new Vector2f(startX, startY);

        window.Draw(rect);
    }

    public void Draw(Drawable drawable)
    {
        window.Draw(drawable);
    }

    public void BeginFrame()
    {
        window.DispatchEvents();
    }
    
    public void BeginDrawing()
    {
        window.Clear(new Color(31, 29, 43));
    }

    public void CompleteDrawing()
    {
        
        window.Display();
    }

    public void CompleteFrame()
    {
        App.InputsHandler.FinishInputs();
    }
    
    public bool WindowShouldClose()
    {
        return window.IsOpen;
    }
}