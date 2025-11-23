using leditor.UI;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace leditor.root;

public sealed class Leditor
{
    private RenderWindow _window = new(new VideoMode(1080, 720), "LEditor");
    private UIEditor _ui;
    private UIHost _host = new(new UIStyle());

    public Leditor()
    {
        _window.SetFramerateLimit(60);
        _window.Closed += (_, _) => _window.Close();
        _window.Resized += 
            (_, args) => _host.SetSize(new Vector2f(args.Width, args.Height));
        _window.KeyPressed +=
            (_, args) => _host.OnKeyPressed(args.Code);
        _window.TextEntered +=
            (_, args) => _host.OnTextEntered(args.Unicode);
        _window.MouseButtonReleased += (_, args) =>
        {
            if (args.Button != Mouse.Button.Left) return;
            _host.OnMouseClick(new Vector2f(args.X, args.Y));
        };
        
        _ui = new UIEditor(_host);
        _host.Root = _ui.Root;
        
        _host.SetSize(Utils.VecU2F(_window.Size));

        var options1 = new AxisBox(_host, UIAxis.Vertical, [
            new UILabel(_host, "Tool 1"),
            new UIButton(_host, "Button 1")
        ]);
        var options2 = new AxisBox(_host, UIAxis.Vertical, [
            new UILabel(_host, "Tool 2"),
            new UIEntry(_host, new UIVar<string>("Entry")),
            new UIButton(_host, "Button 1"),
            new UIButton(_host, "Button 2")
        ]);
        
        _ui.AddTool(() => _ui.ShowToolOptions(options1), new Texture("./assets/tests/floor.png"));
        _ui.AddTool(() => _ui.ShowToolOptions(options2), new Texture("./assets/tests/red.png"));
        _ui.AddTool(() => _ui.CloseToolOptions(), new Texture("./assets/tests/wall_up.png"));
        
        var popup = new AxisBox(_host, UIAxis.Vertical, [
            new UILabel(_host, "Test!"),
            new UIButton(_host, "Close", _ui.ClosePopup)
        ]);
        
        _ui.AddToolPanelCategory("File", new Dictionary<string, Action?>
        { 
            ["Test"] = () => _ui.ShowPopup(popup), 
            ["Exit"] = () => _window.Close()
        });
        
    }

    public void Run()
    {
        while (_window.IsOpen)
        {
            _window.DispatchEvents();
            
            _host.Update(_window);
            
            _window.Clear(Color.Black);
            _host.Draw(_window);
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