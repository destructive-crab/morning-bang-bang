using System.Numerics;
using System.Text;
using Raylib_cs;

namespace leditor.UI;

public class UIEditor
{
    private UIHost _host = new(new UIStyle(
        Color.LightGray,
        Raylib.GetFontDefault(),
        24
    ));

    public UIEditor()
    {
        var root = new AnchorBox(_host);
        
        root.Children.Add(new AnchorBoxChild(
            new Rectangle(10, 10, 0, 0),
            new Rectangle(),
            new UILabel(_host, "Label...")
        ));

        var label = new UILabel(_host, "...and anchor!");
        
        root.Children.Add(new AnchorBoxChild(
            new Rectangle(-label.MinimalSize.X / 2, -label.MinimalSize.Y - 10, 0, 0),
            new Rectangle(.5f, 1, 0, 0),
            label
        ));
        
        _host.Root = root;
        _host.SetSize(new Vector2(
            Raylib.GetRenderWidth(),
            Raylib.GetRenderHeight()
        ));
    }
    
    public void Update()
    {
        if (Raylib.IsWindowResized())
            _host.SetSize(new Vector2(
                Raylib.GetRenderWidth(),
                Raylib.GetRenderHeight()
            ));
    }

    public void Draw()
    {
        _host.Draw();
    }
}