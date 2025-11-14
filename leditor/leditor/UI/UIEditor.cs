using leditor.root;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace leditor.UI;

public class UIEditor
{
    private UIHost _host = new(new UIStyle());
    private AxisBox _axisBox;
    
    public UIEditor(Vector2f size)
    {
        var anchor = new AnchorBox(_host);

        _axisBox = new AxisBox(_host, UIAxis.Vertical, [new UILabel(_host, "Label...")]);
        
        anchor.AddChild(new Anchor(
            new FloatRect(10, 10, 0, 0),
            new FloatRect()
            ), _axisBox
        );

        var label = new UILabel(_host, "...and anchor!");
        
        anchor.AddChild(new Anchor(
            new FloatRect(-label.MinimalSize.X / 2, -label.MinimalSize.Y - 10, 0, 0),
            new FloatRect(.5f, 1, 0, 0)
            ), label
        );

        var buttonsChildren = new List<AUIElement>();
        for (var i = 1; i <= 100; i++)
        {
            var msg = $"Thanks for click! {i}";
            buttonsChildren.Add(new UIButton(_host, $"Click me! {i}    LOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOONG", () => Logger.Info(msg)));
        }
        
        var buttons = new AxisBox(_host, UIAxis.Vertical, buttonsChildren.ToArray());
        var scroll = new ScrollBox(_host, buttons);
        
        var subSplit = new SplitBox(_host, UIAxis.Vertical, anchor, new StackBox(_host, [new UIRect(_host, Color.Green), scroll]), PreserveSide.RightDown);
        _host.Root = new SplitBox(_host, UIAxis.Horizontal, subSplit, new StackBox(_host, [new UIRect(_host, Color.Red)]), PreserveSide.RightDown);
        _host.SetSize(size);
    }

    public void OnResize(Vector2f size)
    {
        _axisBox.AddChild(new UILabel(_host, $"{size.X} {size.Y}"));
        _host.SetSize(size);
    }
    
    public void Update(RenderWindow window)
    {
        _host.Update(window);
    }

    public void Draw(RenderWindow window)
    {
        _host.Draw(window);
    }
}