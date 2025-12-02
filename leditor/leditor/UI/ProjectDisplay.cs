using System.Reflection;
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
        
        App.WindowHandler.window.KeyPressed +=
            (_, args) => host.OnKeyPressed(args.Code);
        App.WindowHandler.window.TextEntered +=
            (_, args) => host.OnTextEntered(args.Unicode);  
    }

    private void BuildContent()
    {
        foreach (Tool tool in projectEnvironment.Toolset.All)
        {
            AddTool(() => SelectToolButton(tool), tool.GetIcon(), "");
        }

        UpdateLeftPanel();
        projectEnvironment.Project.OnEdited += ProjectOnEdited;
        
        AddToolPanelCategory("App", new Dictionary<string, Action?>()
        {
            {"Save (Ctrl + S)", () => App.LeditorInstance.ProjectEnvironment.SaveProject()},
            {"Load (Ctrl + O)", () => App.LeditorInstance.ProjectEnvironment.OpenProjectAtPath(UTLS.ShowOpenProjectDialog())},
            {"Quit (Alt + F4)", () => App.Quit()}
        });
        
        AddToolPanelCategory("Project", new Dictionary<string, Action?>()
        {
            {"Textures", () => ShowPopup(GetTexturesMenu())},
            {"Tiles", () => ShowPopup(GetTilesMenu())},
            {"Units", () => ShowPopup(GetUnisMenu())},
        });
        
        var popup = new AxisBox(host, UIAxis.Vertical, [
            new UILabel(host, "Test!"),
            new UIButton(host, "Close", null)
        ]); 
    }

    private AUIElement GetTilesMenu()
    {
        DataEditor<TileData> editor = new DataEditor<TileData>(projectEnvironment, host);
        editor.OnClosed += ClosePopup;
        return editor.GetDataEditMenu(projectEnvironment.Project.Tiles, OnApply);

        void OnApply(TileData[] data)
        {
            projectEnvironment.Project.RemoveAllTiles();
            projectEnvironment.Project.AddTiles(data);
        }
    }
    
    private AUIElement GetTexturesMenu()
    {
        DataEditor<TextureData> editor = new DataEditor<TextureData>(projectEnvironment, host);
        editor.OnClosed += ClosePopup;
        return editor.GetDataEditMenu(projectEnvironment.Project.Textures, OnApply);

        void OnApply(TextureData[] data)
        {
            projectEnvironment.Project.RemoveAllTextures();
            projectEnvironment.Project.AddTextures(data);
        }
    }
    
    private AUIElement GetUnisMenu()
    {
        DataEditor<UnitData> editor = new(projectEnvironment, host);
        editor.OnClosed += ClosePopup;
        return editor.GetDataEditMenu(projectEnvironment.Project.Units, OnApply);

        void OnApply(UnitData[] data)
        {
            projectEnvironment.Project.RemoveAllUnits();
            projectEnvironment.Project.AddUnits(data);
        }
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
        
        leftPanel.AddChild(new UILabel(host, "\n \n TILEMAPS"));
        
        leftPanel.AddChild(new UILabel(host, "\n \n UNITS"));
        foreach (UnitData unit in projectEnvironment.Project.Units)
        {
            leftPanel.AddChild(new UIButton(host, unit.ID, () => SwitchToUnitButton(unit)));
        }
        leftPanel.AddChild(new UIButton(host, "Create New Unit", CreateNewUnit));
    }

    private void SwitchToUnitButton(UnitData unit)
    {
        projectEnvironment.UnitSwitcher.SwitchTo(unit.ID);
    }
    private void CreateNewUnit()
    {
        projectEnvironment.UnitSwitcher.OpenEmptyUnit($"Unit{Random.Shared.Next(0, 1000)}");
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

class DataEditor<TData>
    where TData : LEditorDataUnit, new()
{
    public event Action OnClosed;
    public event Action<TData[]> OnApply;
    
    private AxisBox dataContainer;
    private AxisBox bottomMenuContainer;
    private StackBox mainMenuContainer;

    private List<DataEditorEntry<TData>> entries = new();
    private UIHost host;

    private ProjectEnvironment ProjectEnvironment;

    public DataEditor(ProjectEnvironment projectEnvironment, UIHost host)
    {
        ProjectEnvironment = projectEnvironment;
        this.host = host;
    }

    public AUIElement GetDataEditMenu(TData[] startValue, Action<TData[]> onApply)
    {
        dataContainer  = new AxisBox  (host, UIAxis.Vertical);
        bottomMenuContainer = new AxisBox  (host, UIAxis.Horizontal);
        mainMenuContainer  = new StackBox (host, [
            new UILimit   (host, new Vector2f(600, App.WindowHandler.Height)),
            new UIRect    (host, new Color(0x343a40FF)),
            new ScrollBox (host, new AxisBox (host, UIAxis.Vertical, [dataContainer, bottomMenuContainer])),
        ]);

        //dataContainer.AddChild(new UIRect(host, host.Style.NormalButton.BgColor));
        
        foreach (TData data in startValue)
        {
            AddNew(data);
        }
        
        bottomMenuContainer.AddChild(new UIButton(host, "New", () => AddNew(new TData())));
        bottomMenuContainer.AddChild(new UIButton(host, "Apply", () => TryApply()));
        bottomMenuContainer.AddChild(new UIButton(host, "Revert", Revert));

        OnApply += onApply;
        
        return mainMenuContainer;
    }

    private void AddNew(TData data)
    {
        DataEditorEntry<TData> entry = new(data, ProjectEnvironment);
        entry.Build(host);
            
        entries.Add(entry);
        dataContainer.AddChild(entry.GetRoot());
    }

    private void Revert()
    {
        CloseEditDataMenu();
    }

    private void CloseEditDataMenu()
    {
        foreach (DataEditorEntry<TData> entry in entries)
        {
            entry.RemoveThis();
        }
        entries.Clear();
        OnClosed?.Invoke();
    }

    private bool TryApply()
    {
        bool isSucceed = true;
        
        foreach (DataEditorEntry<TData> entry in entries)
        {
            if (!entry.ValidateAsSingleData())
            {
                Console.WriteLine(entry.ChangedData.ID + " Invalid change");
                isSucceed = false;
            }
        }

        if (isSucceed)
        {
            TData[] all = GetAllCurrentData();
            foreach (DataEditorEntry<TData> entry in entries)
            {
                if (!UTLS.ValidateRegarding(entry.ChangedData, all))
                {
                    Console.WriteLine(entry.ChangedData.ID + " Invalid change regarding other");
                    entry.MarkAsInvalid();
                    isSucceed = false;
                }
                else if (entry.MarkedAsInvalidExternally)
                {
                    entry.UnMarkAsInvalid();
                }
            }
        }

        if (isSucceed)
        {
            foreach (DataEditorEntry<TData> entry in entries)
            {
                entry.ApplyData();
            }
            OnApply?.Invoke(GetAllCurrentData());
            CloseEditDataMenu();
            return true;
        }

        return false;
    }

    public TData[] GetAllCurrentData()
    {
        List<TData> res = new();
        foreach (DataEditorEntry<TData> entry in entries)
        {
            if (!entry.Removed)
            {
                res.Add(entry.ChangedData);
                Console.WriteLine(entry.ChangedData.ID + " Added");
            }
        }

        return res.ToArray();
    }
}

class DataEditorEntry<TData>
    where TData : LEditorDataUnit, new()
{
    public readonly TData OriginalData;
    public TData ChangedData;
    
    public bool Removed;
    public bool MarkedAsInvalidExternally { get; private set; }

    private AxisBox root;
    private UILabel dataLabel;

    private readonly Dictionary<FieldInfo, UILabel>       labels       = new();
    private readonly Dictionary<FieldInfo, UIImage>       images = new();
    private readonly Dictionary<FieldInfo, UIVar<string>> stringFields = new();
    private readonly Dictionary<FieldInfo, UIVar<string>> intFields    = new();

    private UIButton removeButton;

    private List<AUIElement> content = new();
    private ProjectEnvironment ProjectEnvironment;
    
    public DataEditorEntry(TData originalData, ProjectEnvironment projectEnvironment)
    {
        OriginalData = originalData;
        ProjectEnvironment = projectEnvironment;
        ChangedData = new TData();
        ChangedData.CopyDataFrom(originalData);
    }

    public void Build(UIHost host)
    {
        dataLabel = new UILabel(host, typeof(TData).Name);
        content.Add(dataLabel);

        FieldInfo[] fields = UTLS.GetDeriveOrderedFields(typeof(TData)).ToArray();
        
        foreach (FieldInfo field in fields)
        {
            if (field.IsLiteral) continue;
            if (field.IsInitOnly) continue;
            
            UILabel label = new(host, field.Name);

            UIVar<string> uiVar = new UIVar<string>(field.GetValue(this.OriginalData).ToString());
            
            if (field.FieldType == typeof(int))    intFields.Add(field, uiVar); 
            if (field.FieldType == typeof(string)) stringFields.Add(field, uiVar); 
            
            UIEntry entry = new UIEntry(host, uiVar);
            
            content.Add(label);
            content.Add(entry);

            PathFieldAttribute? pathAttribute = field.GetCustomAttribute<PathFieldAttribute>();
            if (pathAttribute != null)
            {
                content.Add(new UIButton(host, "Choose", () =>
                {
                    string path = UTLS.OpenChooseFileDialog();
                    if (path != String.Empty)
                    {
                        entry.Var.Value = path;
                    }
                }));
            }

            TextureDataReferenceAttribute? textureAttribute = field.GetCustomAttribute<TextureDataReferenceAttribute>();
            if (textureAttribute != null)
            {
                TextureData textureData = ProjectEnvironment.GetTexture(field.GetValue(ChangedData).ToString());

                UIImage image = new UIImage(host, RenderCacher.GetTexture(textureData.PathToTexture), textureData.TextureRect.ToIntRect(), new Vector2i(50, 50));
                images.Add(field, image);
                content.Add(image);
            }
            
            uiVar.OnSet += (value) => ProcessEntry(field, value);
        }

        removeButton = new UIButton(host, "REMOVE", RemoveThis);
        content.Add(removeButton);
        
        content.Add(new UILabel(host, "\n"));
        root = new AxisBox(host, UIAxis.Vertical, content.ToArray());
    }

    public void RemoveThis()
    {
        foreach (KeyValuePair<FieldInfo,UIVar<string>> pair in intFields)
        {
            pair.Value.ClearCallbacks();
        }
        foreach (KeyValuePair<FieldInfo, UIVar<string>> pair in stringFields)
        {
            pair.Value.ClearCallbacks();
        }
        root.RemoveAllChildren();
        Removed = true;
    }

    public bool ApplyData()
    {
        if (ValidateAsSingleData())
        {
            //OriginalData.CopyDataFrom(ChangedData);
            return true;
        }

        return false;
    }

    public bool ValidateAsSingleData()
    {
        if (Removed) return true;

        return ChangedData.ValidateExternalDataChange();
    }

    private void ProcessEntry(FieldInfo field, string value)
    {
        if (field.FieldType == typeof(int))
        {
            if (int.TryParse(value, out int intValue) && (int)field.GetValue(ChangedData) == intValue) 
                return;
            
            field.SetValue(ChangedData, intValue);
            Console.WriteLine($"Set {field.Name} to {value}");
        }
        else if(field.FieldType == typeof(string))
        {
            if ((string)field.GetValue(ChangedData) == value) 
                return;
            
            field.SetValue(ChangedData, value);
            Console.WriteLine($"Set {field.Name} of {ChangedData.ID} to {value}");
        }
    
        if (!ChangedData.ValidateExternalDataChange() && !dataLabel.Text.StartsWith("!"))
        {
            dataLabel.Text = "! " + dataLabel.Text;
        }
        else if (dataLabel.Text.StartsWith("!") && !MarkedAsInvalidExternally)
        {
            dataLabel.Text = dataLabel.Text.Remove(0, 2);
        }

        if (images.TryGetValue(field, out UIImage image))
        {
            TextureData textureData = ProjectEnvironment.GetTexture(field.GetValue(ChangedData).ToString());

            image.Image = RenderCacher.GetTexture(textureData.PathToTexture);
            image.Source = textureData.TextureRect.ToIntRect();
        }

        UpdateVars();
    }

    private void UpdateVars()
    {
        foreach (KeyValuePair<FieldInfo,UIVar<string>> field in intFields)
        {
            field.Value.Value = field.Key.GetValue(ChangedData).ToString();
        }
        
        foreach (KeyValuePair<FieldInfo,UIVar<string>> field in stringFields)
        {
            field.Value.Value = field.Key.GetValue(ChangedData).ToString();
        }
    }

    public void MarkAsInvalid()
    {
        if (!dataLabel.Text.StartsWith("!"))
        {
            dataLabel.Text = "! " + dataLabel.Text;
        }
        MarkedAsInvalidExternally = true;
    }

    public void UnMarkAsInvalid()
    {
        if (dataLabel.Text.StartsWith("!"))
        {
            dataLabel.Text = dataLabel.Text.Remove(0, 2);
        }
        MarkedAsInvalidExternally = false;
    }
    
    public AUIElement GetRoot()
    {
        return root;
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