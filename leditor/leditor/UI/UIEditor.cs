using System.Numerics;
using System.Text;
using leditor.root;
using Raylib_cs;

namespace leditor.UI;

public class UIEditor
{
    private UIHost _host = new(new UIStyle());

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

        var buttons = new AxisBox(_host, UIAxis.Vertical, [
            new UIButton(_host, "Click me!", () => Logger.Info("Thanks for click!")),
            new UIButton(_host, "Click me! 1", () => Logger.Info("Thanks for click! 1")),
            new UIButton(_host, "Click me! 2", () => Logger.Info("Thanks for click! 2")),
            new UIButton(_host, "Click me! 3", () => Logger.Info("Thanks for click! 3")),
        ]) ;
        
        var subSplit = new SplitBox(_host, UIAxis.Vertical, anchor, new UIRect(_host, Color.Green), PreserveSide.RightDown);
        _host.Root = new SplitBox(_host, UIAxis.Horizontal, new StackBox(_host, [new UIRect(_host, Color.Red), buttons]), subSplit);
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