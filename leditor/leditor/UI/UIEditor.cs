
using leditor.root;
using Microsoft.VisualBasic;
using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class UIEditor
{
    public AnchorBox Root;
    
    public UIEditor(UIHost host)
    {
        _host = host;
        Root = new AnchorBox(host);

        var topBarAnchor = new Anchor(
            new FloatRect(0, 0, 0, 24),
            new FloatRect(0, 0, 1, 0)
        );

        _topBar = new AxisBox(_host, UIAxis.Horizontal, []);
        
        var topBar = new StackBox(host, [
            new UIRect(host, new Color(0x495057FF)),
            _topBar
        ]);

        var toolsBarAnchor = new Anchor(
            new FloatRect(-24, 24, 24, -24),
            new FloatRect(1, 0, 0, 1)
        );

        _rightBar = new AxisBox(host, UIAxis.Vertical, []);
        var toolsBar = new StackBox(host, [
            new UIRect(host, new Color(0x495057FF)),
            _rightBar
        ]);
        
        var centerAnchor = new Anchor(
            new FloatRect(0, 24, -24, -24),
            new FloatRect(0, 0, 1, 1)
        );

        var padding = new UIPadding(4, 4, 4, 4);
        
        var leftSpace = new StackBox(host, [
            new UILimit(host, new Vector2f(120, 120)),
            new UIRect(host, new Color(0x343a40FF)),
            new ScrollBox(host, new StackBox(host, [new UILabel(host, "Hello!")], padding))
        ]);
        
        var bottomSpace = new StackBox(host, [
            new UILimit(host, new Vector2f(120, 120)),
            new UIRect(host, new Color(0x343a40FF)),
            new ScrollBox(host, new StackBox(host, [new UILabel(host, "Hello!")], padding))
        ]);
        
        var center = new SplitBox(host, UIAxis.Horizontal, 
            leftSpace,
            new SplitBox(host, UIAxis.Vertical, new UIWorld(host), bottomSpace, PreserveSide.RightDown)
        );
        
        Root.AddChild(topBarAnchor, topBar);
        Root.AddChild(toolsBarAnchor, toolsBar);
        Root.AddChild(centerAnchor, center);
    }

    private UIHost _host;
    private AxisBox _rightBar;
    
    public void AddTool(Action onSelect, Texture texture)
    {
        var scale = new Vector2f(16f / texture.Size.X, 16f / texture.Size.Y);
        _rightBar.AddChild(new UIImageButton(_host, texture, scale, onSelect));
    }

    private readonly AxisBox _topBar;
    private readonly ClickArea _overlayArea = new(default, false);

    public void EnableOverlay(Action action)
    {
        _overlayArea.Rect = Root.Rect;
        _overlayArea.OnClick = action;
        _overlayArea.Overlay = true;
    }

    public void HideOverlay()
    {
        _overlayArea.OnClick = null;
        _overlayArea.Overlay = false;
    }
    
    public void AddToolPanelCategory(string title, Dictionary<string, Action?> actions)
    {
        var category = new ToolPanelCategory(this, _host, title, actions);
        _topBar.AddChild(category.Button);
    }
}

class ToolPanelCategory
{
    private UIEditor _editor;
    private AUIElement _menu;
    public UIButton Button;

    public ToolPanelCategory(UIEditor editor, UIHost host, string title, Dictionary<string, Action?> actions)
    {
        _editor = editor;
        _menu = new StackBox(host, [
            new UIRect(host, new Color(0x495057FF)),
            new AxisBox(host, UIAxis.Vertical,
                actions.AsEnumerable()
                    .Select(pair => new UIButton(host, pair.Key, pair.Value))
                    .ToArray<AUIElement>()
            )
        ]);
        
        Button = new UIButton(host, title, Show);
    }

    private void Show()
    {
        var anchor = new Anchor();
        
        _editor.EnableOverlay(Hide);
        _editor.Root.AddChild(anchor, _menu);
    }

    private void Hide()
    {
        _editor.Root.RemoveChild(_menu);
        _editor.HideOverlay();
    }
}