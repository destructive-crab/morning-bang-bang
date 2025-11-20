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
        _window.Closed += (_, _) => _window.Close();
        _window.Resized += 
            (_, args) => _ui.OnResize(new Vector2f(args.Width, args.Height));
        _window.KeyPressed +=
            (_, args) => _ui.Host.OnKeyPressed(args.Code);
        _window.TextEntered +=
            (_, args) => _ui.Host.OnTextEntered(args.Unicode);
        _window.MouseButtonReleased += (_, args) =>
        {
            if (args.Button != Mouse.Button.Left) return;
            _ui.Host.OnMouseClick(new Vector2f(args.X, args.Y));
        };
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