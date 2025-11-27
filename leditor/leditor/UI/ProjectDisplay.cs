using leditor.root;
using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public class ProjectDisplay : EditorDisplay
{
    public AnchorBox Root;
    public override UIHost Host => host;
    
    private UIHost host;
    
    private AxisBox _rightBar;
    private AxisBox _topBar;
    
    private readonly ClickArea _overlayArea = new(default, false);

    private SingleBox _popupRoot;

    private SingleBox _popupContainer;
    private SingleBox _toolOptionsContainer;

    private StackBox _noneToolOptions;

    private ProjectEnvironment projectEnvironment;
    private StackBox leftSpace;
    private AxisBox leftPanel;

    public ProjectDisplay(ProjectEnvironment projectEnvironment)
    {
        this.projectEnvironment = projectEnvironment;
       
        BuildBase();
        BuildContent();
    }

    private void BuildContent()
    {
        foreach (Tool tool in projectEnvironment.Toolset.All)
        {
            AddTool(() => SelectToolButton(tool), tool.GetIcon(), "");
        }

        UpdateLeftPanel();
        projectEnvironment.Project.OnEdited += ProjectOnEdited;
    }

    private void ProjectOnEdited(object arg1, object arg2)
    {
        if (arg2 is UnitData || arg2 is TilemapData)
        {
            UpdateLeftPanel();
        }
    }

    private void SelectToolButton(Tool tool)
    {
        projectEnvironment.Toolset.SelectTool(tool);
        UpdateToolOptions();
    }

    private void UpdateToolOptions() 
        => ShowToolOptions(projectEnvironment.Toolset.CurrentTool.BuildGUI(host));

    private void UpdateLeftPanel()
    {
        leftPanel.RemoveAllChildren();
        foreach (UnitData unit in projectEnvironment.Project.Units)
        {
            leftPanel.AddChild(new UIButton(host, unit.UnitID, () => SwitchToUnitButton(unit)));
        }
        leftPanel.AddChild(new UIButton(host, "Create New Unit", CreateNewUnit));
    }

    private void SwitchToUnitButton(UnitData unit)
    {
        projectEnvironment.UnitSwitcher.SwitchTo(unit.UnitID);
    }
    private void CreateNewUnit()
    {
        projectEnvironment.UnitSwitcher.OpenEmptyUnit($"Unit{Random.Shared.Next(0, 10)}");
    }

    public void AddTool(Action onSelect, Texture texture, string name)
    {
        var scale = new Vector2f(16f / texture.Size.X, 16f / texture.Size.Y);
        _rightBar.AddChild(new UIImageButton(host, texture, new Rect(0,0, (int)texture.Size.X, (int)texture.Size.Y), name, scale, onSelect));
    }

    public void EnableOverlay(Action action)
    {
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
        var category = new ToolPanelCategory(this, host, title, actions);
        _topBar.AddChild(category.Button);
    }
    
    public void ShowPopup(AUIElement popup)
    {
        _popupContainer.Child = popup;
        _popupRoot.Hide = false;
    }

    public void ClosePopup()
    {
        _popupRoot.Hide = true;
        _popupContainer.Child = null;
    }

    public void ShowToolOptions(AUIElement toolOptions)
    {
        _toolOptionsContainer.Child = toolOptions;
    }

    public void CloseToolOptions()
    {
        _toolOptionsContainer.Child = _noneToolOptions;
    }

    private void BuildBase()
    {
        host = new UIHost(new UIStyle());
        Root = new AnchorBox(host);

        var topBarAnchor = new Anchor(
            new FloatRect(0, 0, 0, 24),
            new FloatRect(0, 0, 1, 0)
        );

        _topBar = new AxisBox(host, UIAxis.Horizontal, []);
        
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

        leftPanel = new AxisBox(host, UIAxis.Vertical);
       
        leftSpace = new StackBox(host, [
            new UILimit(host, new Vector2f(120, 120)),
            new UIRect(host, new Color(0x343a40FF)),
            new ScrollBox(host, leftPanel)
        ]);
        
        _noneToolOptions = new StackBox(host, [new UILabel(host, "None selected.")], centerX: true, centerY: true);
        _toolOptionsContainer = new SingleBox(host, _noneToolOptions);
        
        var bottomSpace = new StackBox(host, [
            new UILimit(host, new Vector2f(120, 120)),
            new UIRect(host, new Color(0x343a40FF)),
            new ScrollBox(host, new StackBox(host, [_toolOptionsContainer], padding))
        ]);
        
        var center = new SplitBox(host, UIAxis.Horizontal, 
            leftSpace,
            new SplitBox(host, UIAxis.Vertical, new UIWorld(host), bottomSpace, PreserveSide.RightDown)
        );
        
        Root.AddChild(topBarAnchor, topBar);
        Root.AddChild(toolsBarAnchor, toolsBar);
        Root.AddChild(centerAnchor, center);

        var overlayAnchor = new Anchor
        {
            Relative = new FloatRect(0, 0, 1, 1)
        };
        Root.AddChild(overlayAnchor, new UIClickArea(host, _overlayArea));
        
        _popupContainer = new SingleBox(host);
        _popupRoot = new SingleBox(host, 
            new StackBox(host, [
                new UIRect(host, new Color(0x000000AA)),
                new UIClickArea(host, new ClickArea(default)),
                new StackBox(host, [new StackBox(host, [
                    new UIRect(host, new Color(0x343a40FF)),
                    new StackBox(host, [_popupContainer], padding)
                ])], centerX: true, centerY: true)
            ]), true
        );
        
        Root.AddChild(overlayAnchor, _popupRoot);
        host.Root = Root;
    }
}

class ToolPanelCategory
{
    private readonly ProjectDisplay _editor;
    private readonly AUIElement _menu;
    public readonly UIButton Button;

    public ToolPanelCategory(ProjectDisplay editor, UIHost host, string title, Dictionary<string, Action?> actions)
    {
        _editor = editor;
        _menu = new StackBox(host, [
            new UIRect(host, new Color(0x495057FF)),
            new AxisBox(host, UIAxis.Vertical,
                actions.AsEnumerable()
                    .Select(pair => new UIButton(host, pair.Key, () =>
                    {
                        Hide();
                        pair.Value?.Invoke();
                    }))
                    .ToArray<AUIElement>()
            )
        ]);
        
        Button = new UIButton(host, title, Show);
    }

    private void Show()
    {
        var anchor = new Anchor();
        anchor.BaseRect.Top = Button.Rect.Top + Button.Rect.Height;

        var maxLeft = float.Max(0, _editor.Root.Rect.Left + _editor.Root.Rect.Width - _menu.MinimalSize.X);
        anchor.BaseRect.Left = float.Min(Button.Rect.Left, maxLeft);
        
        _editor.EnableOverlay(Hide);
        _editor.Root.AddChild(anchor, _menu);
    }

    private void Hide()
    {
        _editor.Root.RemoveChild(_menu);
        _editor.HideOverlay();
    }
}