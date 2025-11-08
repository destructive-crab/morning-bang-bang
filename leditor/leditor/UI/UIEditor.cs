using System.Numerics;
using System.Text;
using Raylib_cs;

namespace leditor.UI;

public class UIEditor
{
    private UIHost _host = new(new UIStyle(
        Color.LightGray,
        Raylib.GetFontDefault(),
        24, 2
    ));

    private AxisBox _axisBox;

    public UIEditor()
    {
        var anchor = new AnchorBox(_host);

        _axisBox = new AxisBox(_host, UIAxis.Vertical, [new UILabel(_host, "Label...")]);
        
        anchor.AddChild(new Anchor(
            new Rectangle(10, 10, 0, 0),
            new Rectangle()
            ), _axisBox
        );

        var label = new UILabel(_host, "...and anchor!");
        
        anchor.AddChild(new Anchor(
            new Rectangle(-label.MinimalSize.X / 2, -label.MinimalSize.Y - 10, 0, 0),
            new Rectangle(.5f, 1, 0, 0)
            ), label
        );

        var padding = new UIPadding(200, 200, 10, 10);
        var stack = new StackBox(_host, [anchor], padding);
        var subSplit = new SplitBox(_host, UIAxis.Vertical, stack, new UIRect(_host, Color.Green), PreserveSide.RightDown);
        _host.Root = new SplitBox(_host, UIAxis.Horizontal, new UIRect(_host, Color.Red), subSplit);
        _host.SetSize(new Vector2(
            Raylib.GetRenderWidth(),
            Raylib.GetRenderHeight()
        ));
    }
    
    public void Update()
    {
        if (Raylib.IsWindowResized())
        {
            _axisBox.AddChild(new UILabel(_host, $"{Raylib.GetRenderWidth()} {Raylib.GetRenderHeight()}"));
            _host.SetSize(new Vector2(
                Raylib.GetRenderWidth(),
                Raylib.GetRenderHeight()
            ));
        }
        
        _host.Update();
    }

    public void Draw()
    {
        _host.Draw();
    }
}