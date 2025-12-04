using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace leditor.root;

public sealed class InputsHandler
{
    public float MouseWheelDelta { get; private set; }
    
    public bool IsRightMouseButtonPressed { get; private set; }
    public bool IsLeftMouseButtonPressed { get; private set; }

    public bool IsRightMouseButtonDown { get; private set; }
    public bool IsLeftMouseButtonDown { get; private set; }
    
    public bool IsRightMouseButtonReleased { get; private set; }
    public bool IsLeftMouseButtonReleased { get; private set; }
    
    public Vector2f WorldMousePosition => window.MapPixelToCoords(Mouse.GetPosition(window));
    public Vector2i MousePosition => Mouse.GetPosition(window);
    
    private RenderWindow window;

    private readonly List<Keyboard.Key> keysDown = new();
    private readonly List<Keyboard.Key> stillPressed = new();

    public InputsHandler(RenderWindow window)
    {
        this.window = window;
        
        //bind events
        window.MouseWheelScrolled += OnMouseWheelScrolled;
        window.MouseButtonPressed += OnMouseButtonPressed;
        window.MouseButtonReleased += OnMouseButtonReleased;
        
        window.KeyPressed += OnKeyPressed;
        window.KeyReleased += OnKeyReleased;

        window.Closed += WindowOnClosed; 
    }

    public bool IsKeyDown(Keyboard.Key key)
    {
        return keysDown.Contains(key);
    }

    public bool IsKeyPressed(Keyboard.Key key)
    {
        return stillPressed.Contains(key);
    }

    private void OnKeyReleased(object? sender, KeyEventArgs e)
    {
        stillPressed.Remove(e.Code);
    }

    private void OnKeyPressed(object? sender, KeyEventArgs e)
    {
        keysDown.Add(e.Code);
        stillPressed.Add(e.Code);
    }

    private void WindowOnClosed(object? sender, EventArgs e)
    {
        window.Close();
    }

    private void OnMouseButtonPressed(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button == Mouse.Button.Right)
        {
            if (!IsRightMouseButtonPressed)
            {
                IsRightMouseButtonDown = true;
            }

            IsRightMouseButtonPressed = true;
        }
        else if (e.Button == Mouse.Button.Left)
        {
            if (!IsLeftMouseButtonPressed)
            {
                IsLeftMouseButtonDown = true;
            }

            IsLeftMouseButtonPressed = true;
        }
    }

    private void OnMouseButtonReleased(object? sender, MouseButtonEventArgs e)
    {
        if (e.Button == Mouse.Button.Right)
        {
            IsRightMouseButtonReleased = true;
            IsRightMouseButtonPressed = false;
        }
        else if (e.Button == Mouse.Button.Left)
        {
            IsLeftMouseButtonReleased = true;
            IsLeftMouseButtonPressed = false;
        }
    }

    private void OnMouseWheelScrolled(object? sender, MouseWheelScrollEventArgs e)
    {
        MouseWheelDelta = e.Delta;
    }

    public void FinishInputs()
    {
        IsLeftMouseButtonReleased = false;
        IsRightMouseButtonReleased = false;
        
        IsLeftMouseButtonDown = false;
        IsRightMouseButtonDown = false;

        MouseWheelDelta = 0;
        
        keysDown.Clear();
    }
}