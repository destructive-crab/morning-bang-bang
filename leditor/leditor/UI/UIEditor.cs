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
        
        void ButtonAction()
            => Logger.Info("Thanks for click!");
        
        var button = new UIButton(_host, "Click me!", ButtonAction);
        
        anchor.AddChild(new Anchor(
            new Rectangle(7, -40, 0, 0),
            new Rectangle(0, 1, 0, 0)
        ), button);
        
        var subSplit = new SplitBox(_host, UIAxis.Vertical, anchor, new UIRect(_host, Color.Green), PreserveSide.RightDown);
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