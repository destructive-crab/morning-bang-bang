using leditor.UI;
using SFML.Graphics;
using SFML.Window;

namespace leditor.root;

public sealed class Leditor
{
    private RenderWindow _window = new(new VideoMode(1080, 720), "LEditor");
    private UIEditor _ui;

    public Leditor()
    {
        _ui = new UIEditor(_window);
        
        _window.SetFramerateLimit(60);
        _window.Closed += WindowOnClosed;
        _window.Resized += _ui.OnResize;
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