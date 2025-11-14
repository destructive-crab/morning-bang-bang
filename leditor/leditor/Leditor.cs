using leditor.UI;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace leditor.root;

public sealed class Leditor
{
    private RenderWindow _window = new(new VideoMode(1080, 720), "LEditor");
    private UIEditor _ui;

    public Leditor()
    {
        _ui = new UIEditor(new Vector2f(_window.Size.X, _window.Size.Y));
        
        _window.SetFramerateLimit(60);
        _window.Closed += WindowOnClosed;
        _window.Resized += 
            (_, args) => _ui.OnResize(new Vector2f(args.Width, args.Height));
    }

    private void WindowOnClosed(object? sender, EventArgs e)
    {
        _window.Close();
    }

    public void Run()
    {
        while (_window.IsOpen)
        {
            _window.DispatchEvents();
            
            _ui.Update(_window);
               
            _window.Clear(Color.Black);
            _ui.Draw(_window);
            _window.Display();
        }
    }

    public static void Main()
    {
        Logger.MinimumLevel = Logger.Level.Debug;
        
        var editor = new Leditor();
        editor.Run();
    }
}