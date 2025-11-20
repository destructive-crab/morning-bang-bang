using leditor.root;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace leditor.UI;

public class UIEditor
{
    public readonly UIHost Host = new(new UIStyle());
    private AxisBox _axisBox;
    
    public UIEditor(Vector2f size)
    {
        var anchor = new AnchorBox(Host);

        _axisBox = new AxisBox(Host, UIAxis.Vertical, [new UILabel(Host, "Label...")]);
        
        anchor.AddChild(new Anchor(
            new FloatRect(10, 10, 0, 0),
            new FloatRect()
            ), _axisBox
        );

        var label = new UILabel(Host, "...and anchor!");
        
        anchor.AddChild(new Anchor(
            new FloatRect(-label.MinimalSize.X / 2, -label.MinimalSize.Y - 10, 0, 0),
            new FloatRect(.5f, 1, 0, 0)
            ), label
        );

        var buttonsChildren = new List<AUIElement>();
        for (var i = 1; i <= 100; i++)
        {
            var msg = $"Thanks for click! {i}";
            buttonsChildren.Add(new UIButton(Host, $"Click me! {i}    LOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOONG", () => Logger.Info(msg)));
        }
        
        var buttons = new AxisBox(Host, UIAxis.Vertical, buttonsChildren.ToArray());
        var scroll = new ScrollBox(Host, buttons);

        
        var entryVar = new UIVar<string>("Linked entry");

        entryVar.OnSet += val => Logger.Debug(val);
        
        var onlyIntVar = new UIVar<string>("0");

        var lastValue = "0";
        onlyIntVar.OnSet += newString =>
        {
            if (int.TryParse(newString, out _))
                lastValue = newString;
            else onlyIntVar.Value = lastValue;
        };
        
        var right = new AxisBox(Host, UIAxis.Vertical, [
            new UIEntry(Host, entryVar), 
            new UIEntry(Host, entryVar), 
            new UIEntry(Host, onlyIntVar)
        ]);
        
        var subSplit = new SplitBox(Host, UIAxis.Vertical, anchor, new StackBox(Host, [new UIRect(Host, Color.Green), scroll]), PreserveSide.RightDown);
        Host.Root = new SplitBox(Host, UIAxis.Horizontal, subSplit, new StackBox(Host, [new UIRect(Host, Color.Red), right]), PreserveSide.RightDown);
        Host.SetSize(size);
    }

    public void OnResize(Vector2f size)
    {
        _axisBox.AddChild(new UILabel(Host, $"{size.X} {size.Y}"));
        Host.SetSize(size);
    }
    
    public void Update(RenderWindow window)
    {
        Host.Update(window);
    }

    public void Draw(RenderWindow window)
    {
        Host.Draw(window);
    }
}