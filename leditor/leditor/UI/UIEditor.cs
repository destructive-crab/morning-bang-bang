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
        var anchor = new AnchorBox(_host);
        
        anchor.AddChild(new Anchor(
            new Rectangle(10, 10, 0, 0),
            new Rectangle()
            ), new UILabel(_host, "Label...")
        );

        var label = new UILabel(_host, "...and anchor!");
        
        anchor.AddChild(new Anchor(
            new Rectangle(-label.MinimalSize.X / 2, -label.MinimalSize.Y - 10, 0, 0),
            new Rectangle(.5f, 1, 0, 0)
            ), label
        );

        var padding = new UIPadding(200, 200, 10, 10);
        
        _host.Root = new StackBox(_host, [anchor], padding);
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
        
        _host.Update();
    }

    public void Draw()
    {
        _host.Draw();
    }
}