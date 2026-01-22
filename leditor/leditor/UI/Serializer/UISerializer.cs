using leditor.UI;
using SFML.Graphics;
using SFML.System;

namespace leditor.root.UI.Serializer;

public class UISerializer
{
    
}

public sealed class TextureReference : AUIElement
{
    private readonly UILabel uiLabel;
    private readonly UIEntry entry;
    
    /// <summary>
    /// provide project data to show checks
    /// </summary>
    public TextureReference(string label, string startValue, ProjectData project = null) : base(App.UIHost, new Vector2f(0,0))
    {
        uiLabel = new UILabel(Host, label);
        entry = new UIEntry(Host, new UIVar<string>(startValue));
        MinimalSize = new Vector2f(100, uiLabel.MinimalSize.Y + entry.MinimalSize.Y);
    }

    public override void UpdateLayout()
    {
        uiLabel.Rect = new FloatRect(Rect.Position, new Vector2f(Rect.Size.X, uiLabel.Rect.Size.Y));
        entry.Rect = new FloatRect(Rect.Position + new Vector2f(0, uiLabel.Rect.Size.Y), new Vector2f(Rect.Size.X, entry.Rect.Size.Y));
    }

    public override void ProcessClicks()
    {
        base.ProcessClicks();
        entry.ProcessClicks();
    }

    public override void Draw(RenderTarget target)
    {
        uiLabel.Draw(target);
        entry.Draw(target);
    }
}

public sealed class PathEntry : AUIElement
{
    public UIVar<string> PathVar { get; private set; }
    
    private UILabel label;
    private UIButton chooseButton;
    private UIEntry pathEntry;

    private const int LABEL_Y_OFFSET = 5;

    public PathEntry(string name, string startValue = "", int width = 100) : base(App.UIHost, new(width, App.UIHost.Style.NormalButton().BottomHeight + App.UIHost.Style.FontSize() * 2))
    {
        label = new UILabel(App.UIHost, name);
        chooseButton = new UIButton(App.UIHost, "Choose", ChoosePath);
        PathVar = new UIVar<string>(startValue);
        pathEntry = new UIEntry(App.UIHost, PathVar);
        MinimalSize = new Vector2f(100, label.MinimalSize.Y + pathEntry.MinimalSize.Y + LABEL_Y_OFFSET * 2);
        Rect = new FloatRect(new Vector2f(0, 0), MinimalSize);
    }

    private void ChoosePath()
    {
        string path = UTLS.OpenChooseFileDialog();
        if (path != String.Empty)
        {
            PathVar.Value = path;
        }
    }

    public override void UpdateLayout()
    {
        label.Rect = new FloatRect(Rect.Position, label.MinimalSize);
        int labelHeight = (int)label.MinimalSize.Y;
        pathEntry.Rect = new FloatRect(Rect.Position + new Vector2f(0, labelHeight + LABEL_Y_OFFSET), new Vector2f(Rect.Size.X - chooseButton.MinimalSize.X - 10, chooseButton.MinimalSize.Y));
        chooseButton.Rect = new FloatRect(pathEntry.Rect.Position + new Vector2f(pathEntry.Rect.Size.X, 0), chooseButton.Rect.Size);
        
        label.UpdateLayout();
        pathEntry.UpdateLayout();
        chooseButton.UpdateLayout();
    }

    public override void ProcessClicks()
    {
        base.ProcessClicks();
        pathEntry.ProcessClicks();
        chooseButton.ProcessClicks();
    }

    public override void Draw(RenderTarget target)
    {
        label.Draw(target);
        pathEntry.Draw(target);
        chooseButton.Draw(target);
    }
}
public sealed class UIIntVecEntry : AUIElement
{
    public readonly UIVar<string> X;
    public readonly UIVar<string> Y;

    private readonly AUIElement root;
    
    public UIIntVecEntry(string name, int x, int y, int width = 300) 
        : base(App.UIHost, new Vector2f(App.UIHost.Style.FontSize() * 2, width))
    {
        X = new UIVar<string>(x.ToString());
        Y = new UIVar<string>(y.ToString());

        UILabel xLabel = new UILabel(App.UIHost, "X:");
        UILabel yLabel = new UILabel(App.UIHost, "Y:");

        int entryWidth = (int)(width - xLabel.MinimalSize.X * 2) / 2;
        
        root = new AxisBox(App.UIHost, UIAxis.Vertical,
                    new UILabel(App.UIHost, name),
                    new AxisBox(App.UIHost, UIAxis.Horizontal, true, 
                        xLabel, new UIEntry(App.UIHost, X), 
                        yLabel, new UIEntry(App.UIHost, Y)));
    }

    public override void ProcessClicks()
    {
        base.ProcessClicks();
        root.ProcessClicks();
    }

    public override void UpdateLayout()
    {
        root.Rect = Rect;
    }

    public override void Draw(RenderTarget target)
    {
        root.Draw(target);
    }
}