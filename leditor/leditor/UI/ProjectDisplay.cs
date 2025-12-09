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

    private UIWorld world;
    private LeftPanel leftPanel;

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

    public override void Tick()
    {
        if(App.LeditorInstance.buffer != null)
            App.LeditorInstance.buffer.BlockInputs = !world.IsHovered;
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
            {"Save    (Ctrl + S)", () => App.LeditorInstance.ProjectEnvironment.SaveProject()},
            {"Save As (Shift + Ctrl + S)", () => App.LeditorInstance.ProjectEnvironment.SaveProjectAtPath(UTLS.OpenSaveProjectDialog())},
            {"Load    (Ctrl + O)", () => App.LeditorInstance.OpenProject(UTLS.ShowOpenProjectDialog())},
            {"New     (Ctrl + N)", () => App.LeditorInstance.OpenProject(string.Empty)},
            {"Quit    (Alt + F4)", () => App.Quit()}
        });
        
        AddToolPanelCategory("Project", new Dictionary<string, Action?>()
        {
            {"Textures", () => ShowPopup(GetTexturesMenu())},
            {"Tiles", () => ShowPopup(GetTilesMenu())},
            {"Maps", () => ShowPopup(GetMapsMenu())},
            {"Units", () => ShowPopup(GetUnitsMenu())},
        });
    }

    private AUIElement GetMapsMenu()
    {
        DataEditor<MapData> editor = new DataEditor<MapData>(projectEnvironment, host);
        editor.OnClosed += ClosePopup;
        return editor.GetDataEditMenu(projectEnvironment.Project.Maps, OnApply);

        void OnApply(MapData[] data)
        {
            projectEnvironment.Project.RemoveAllMaps();
            projectEnvironment.Project.AddMaps(data);
        }
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
    
    private AUIElement GetUnitsMenu()
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

    private void ProjectOnEdited(ProjectData.EditEntry editEntry)
    {
        if (editEntry.Who is UnitData || editEntry.Who is MapData)
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
        => ShowToolOptions(projectEnvironment.Toolset.CurrentTool.BuildUI(host));

    private void UpdateLeftPanel()
    {
        leftPanel.UpdateContent();
    }

    private void SwitchToUnitButton(UnitData unit)
    {
        //projectEnvironment.UnitSwitcher.SwitchTo(unit.ID);
    }
    private void CreateNewUnit()
    {
      //  projectEnvironment.UnitSwitcher.OpenEmptyUnit($"Unit{Random.Shared.Next(0, 1000)}");
    }
    
    public void AddTool(Action onSelect, Texture texture, string name)
    {
        var scale = new Vector2f(16f / texture.Size.X, 16f / texture.Size.Y);
        _rightBar.AddChild(new UIImageButton(host, texture, new Rect(0,0, (int)texture.Size.X, (int)texture.Size.Y), scale, onSelect));
    }

    public void EnableOverlay(Action action)
    {
        _overlayArea.OnRightMouseButtonClick = action;
        _overlayArea.Overlay = true;
    }

    public void HideOverlay()
    {
        _overlayArea.OnRightMouseButtonClick = null;
        _overlayArea.Overlay = false;
    }

    public void AddToolPanelCategory(string title, Dictionary<string, Action?> actions)
    {
        ToolPanelCategory category = new ToolPanelCategory(this, host, title, actions);
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
        host = App.UIHost;
        Root = new AnchorBox(host);

        var topBarAnchor = new Anchor(
            new FloatRect(0, 0, 0, 40),
            new FloatRect(0, 0, 1, 0)
        );

        _topBar = new AxisBox(host, UIAxis.Horizontal, []);
        
        var topBar = new StackBox(host, [
            new UIRect(host),
            _topBar
        ]);

        var toolsBarAnchor = new Anchor(
            new FloatRect(-24, topBarAnchor.BaseRect.Height, 24, -24),
            new FloatRect(1, 0, 0, 1)
        );

        _rightBar = new AxisBox(host, UIAxis.Vertical, []);
        var toolsBar = new StackBox(host, [
            new UIRect(host),
            _rightBar
        ]);
        
        var centerAnchor = new Anchor(
            new FloatRect(0, topBarAnchor.BaseRect.Height, -24, -24),
            new FloatRect(0, 0, 1, 1)
        );

        var padding = new UIPadding(4, 4, 4, 4);

        
        _noneToolOptions = new StackBox(host, [new UILabel(host, "None selected.")], centerX: true, centerY: true);
        _toolOptionsContainer = new SingleBox(host, _noneToolOptions);
        
        var bottomSpace = new StackBox(host, [
            new UILimit(host, new Vector2f(120, 120)),
            new UIRect(host),
            new ScrollBox(host, new StackBox(host, [_toolOptionsContainer], padding))
        ]);

        world = new UIWorld(host);
        leftPanel = new LeftPanel(projectEnvironment, this);
        var center = new SplitBox(host, UIAxis.Horizontal, 
            leftPanel.GetRoot(),
            new SplitBox(host, UIAxis.Vertical, world, bottomSpace, PreserveSide.RightDown)
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
                new UIRect(host, new Color(0x00000080)),
                new UIClickArea(host, new ClickArea(default)),
                new StackBox(host, [new StackBox(host, [
                    new UIRect(host),
                    new StackBox(host, [_popupContainer], padding)
                ])], centerX: true, centerY: true)
            ]), true
        );
        
        Root.AddChild(overlayAnchor, _popupRoot);
        host.SetRoot(Root);
    }
}

class DataEditor<TData>
    where TData : LEditorDataUnit, new()
{
    public event Action OnClosed;
    public event Action<TData[]> OnApply;
    
    private AxisBox dataContainer;
    private AxisBox bottomMenuContainer;
    private AxisBox mainMenuContainer;

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
        bottomMenuContainer = new AxisBox  (host, UIAxis.Vertical);
        mainMenuContainer  = new AxisBox(host, UIAxis.Horizontal, new StackBox (host, [
            new UILimit   (host, new Vector2f(600, App.WindowHandler.Height)),
            new UIRect    (host),
            new ScrollBox (host, new AxisBox (host, UIAxis.Vertical, [dataContainer, ])),
        ]), bottomMenuContainer);

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

    private void AddSeparatorToDataContainer()
    {
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
                entry.CanApplyData();
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
    private UIHost host;

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

    public void Build(UIHost host, CustomDataDrawer<TData> customDataDrawer = null)
    {
        this.host = host;
        dataLabel = new UILabel(host, typeof(TData).Name);
        content.Add(dataLabel);

        FieldInfo[] fields = UTLS.GetDeriveOrderedFields(typeof(TData)).ToArray();

        foreach (FieldInfo field in fields)
        {
            if (field.IsLiteral) continue;
            if (field.IsInitOnly) continue;

            UIVar<string> uiVar = new UIVar<string>(field.GetValue(this.OriginalData).ToString());

            if (field.FieldType == typeof(int)) intFields.Add(field, uiVar);
            if (field.FieldType == typeof(string)) stringFields.Add(field, uiVar);

            PathFieldAttribute? pathAttribute = field.GetCustomAttribute<PathFieldAttribute>();
            XYAxisStartAttribute? xyAxisStartAttribute = field.GetCustomAttribute<XYAxisStartAttribute>();
            TextureDataReferenceAttribute? textureAttribute = field.GetCustomAttribute<TextureDataReferenceAttribute>();
            
            if (pathAttribute != null)
            {
                BuildPathField(field, uiVar, content);
            }
            else if (xyAxisStartAttribute != null)
            {
                BuildAxisField(field, xyAxisStartAttribute, uiVar, content);
            }
            else if (textureAttribute != null)
            {
                BuildTextureReferenceField(field, uiVar, content);
            }
            else
            {
                UILabel label = new(host, field.Name);
                content.Add(label);
                UIEntry entry = new UIEntry(host, uiVar, 215);
                content.Add(entry);
            }
            
            uiVar.OnSet += (value) => ProcessEntry(field, value);
        }

        removeButton = new UIButton(host, "REMOVE", RemoveThis);
        content.Add(removeButton);
        content.Add(new UISpace(host, new Vector2f(0, 10)));
        
        root = new AxisBox(host, UIAxis.Vertical, content.ToArray());
    }

    private void BuildTextureReferenceField(FieldInfo field, UIVar<string> var, List<AUIElement> content)
    {
        TextureData textureData = ProjectEnvironment.GetTexture(field.GetValue(ChangedData).ToString());

        UILabel label = new(host, field.Name);
        UIEntry entry = new UIEntry(host, var);
        UIImage image = new UIImage(host, RenderCacher.GetTexture(textureData.PathToTexture), textureData.TextureRect.ToIntRect(), new Vector2i(50, 50));
        
        images.Add(field, image);
        content.Add(label);
        content.Add(entry);
        content.Add(image);
    }

    AxisBox? xyAxisContainer = null;
    private void BuildAxisField(FieldInfo field, XYAxisStartAttribute xyAxisStartAttribute, UIVar<string> var, List<AUIElement> content)
    {
        UIEntry entry = new UIEntry(host, var, 110);
        if (xyAxisContainer == null)
        {
            UILabel label = new(host, xyAxisStartAttribute.LabelName);
            
            content.Add(label);
            xyAxisContainer = new AxisBox(host, UIAxis.Horizontal);
            xyAxisContainer.AddChild(new UILabel(host, "X:"));
            xyAxisContainer.AddChild(entry);
            content.Add(xyAxisContainer);   
        }
        else
        {
            xyAxisContainer.AddChild(new UILabel(host, "Y:"));
            xyAxisContainer.AddChild(entry);
            xyAxisContainer = null;
        }
    }

    private void BuildPathField(FieldInfo field, UIVar<string> var, List<AUIElement> content)
    {
        UIEntry entry = new UIEntry(host, var, 480);
        UILabel label = new(host, field.Name);
        
        content.Add(label);
        content.Add(new AxisBox(host, UIAxis.Horizontal,
            entry,
            new UIButton(host, "Choose", () =>
            {
                string path = UTLS.OpenChooseFileDialog();
                if (path != String.Empty)
                {
                    entry.Var.Value = path;
                }
            })));
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

    public bool CanApplyData()
    {
        if (ValidateAsSingleData())
        {
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
            new UIRect(host, null, new Vector2f(4, 4)),
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

//todo: normal serialization logic
sealed class EnterDataPopup
{
    private readonly Dictionary<string, UIVar<string>> vars = new();
    private Func<bool, string> OnEntered;

    private StackBox root;
    private AxisBox list;
    private UILabel message;
    
    public EnterDataPopup(Func<bool, string> onEntered, string label, params string[] keys)
    {
        OnEntered += onEntered;

        list = new AxisBox(App.UIHost, UIAxis.Vertical);
        list.AddChild(new UILabel(App.UIHost, label));

        foreach (string key in keys)
        {
            UIVar<string> var = new UIVar<string>("");
            UIEntry entry = new UIEntry(App.UIHost, var);
            vars.Add(key, var);

            list.AddChild(new UILabel(App.UIHost, key));
            list.AddChild(entry);
        }

        message = new UILabel(App.UIHost, "");
        
        list.AddChild(new AxisBox(App.UIHost, UIAxis.Horizontal, 
            new UIButton(App.UIHost, "Apply", () => message.Text = OnEntered?.Invoke(true)),
            new UIButton(App.UIHost, "Cancel", () => message.Text = OnEntered?.Invoke(false))));

        list.AddChild(message);
        
        root = new StackBox(App.UIHost, [
            new UIRect(App.UIHost),
            list]);
    }

    public string Get(string id)
    {
        return vars[id].Value;
    }

    public AUIElement GetRoot()
    {
        return root;
    }
}

sealed class LeftPanel
{
    public AUIElement GetRoot() => leftSpace;
    
    private ProjectEnvironment env;
    
    private StackBox leftSpace;
    
    private AxisBox leftPanel;
    
    private UISelectionList mapsList;
    private UISelectionList unitsList;

    private AxisBox controlsList;

    private readonly Dictionary<string, UIButton> mapButtons = new();
    private readonly Dictionary<string, UIButton> unitButtons = new();

    private UIButton createMapButton;
    private UIButton createUnitButton;

    private ProjectDisplay display;
     
    public LeftPanel(ProjectEnvironment env, ProjectDisplay display)
    {
        this.env = env;
        this.display = display;

        mapsList = new UISelectionList(App.UIHost);
        unitsList = new UISelectionList(App.UIHost);
        controlsList = new AxisBox(App.UIHost, UIAxis.Vertical);
        
        createMapButton = new UIButton(App.UIHost, "New Map", CreateMapCallback);
        createUnitButton = new UIButton(App.UIHost, "New Unit", CreateUnitCallback);

        leftPanel = new AxisBox(App.UIHost, UIAxis.Vertical, [
            new UILabel(App.UIHost, "Maps"),
            mapsList,
            createMapButton,
            
            new UILabel(App.UIHost, "Units"),
            unitsList,
            createUnitButton]);
        
        //removeMapButton = new UIButton(App.UIHost, "Remove Map", RemoveMapCallback);
       
        leftSpace = new StackBox(App.UIHost, [
            new UILimit(App.UIHost, new Vector2f(120, 120)),
            new UIRect(App.UIHost),
            new ScrollBox(App.UIHost, leftPanel)
        ]);
    }

    public void UpdateContent()
    {
        UpdateUnit(env.Project.Maps, mapButtons, mapsList, env.Project.GetMap, (d) => Create(d as MapData));
        UpdateUnit(env.Project.Units, unitButtons, unitsList, env.Project.GetUnit, (d) => Create(d as UnitData));
    }

    private void UpdateUnit(LEditorDataUnit[] data, Dictionary<string, UIButton> buttons, UISelectionList list, Func<string, object?> dataGetter, Func<LEditorDataUnit, UIOption> createButton)
    {
        List<string> removed = new();
        List<KeyValuePair<string, UIButton>> added = new();
        
        //defining which buttons we need to remove
        foreach (KeyValuePair<string,UIButton> pair in buttons)
        {
            if (dataGetter.Invoke(pair.Key) == null)
            {
                list.RemoveChild(pair.Value);
                removed.Add(pair.Key);
            }
        }
        //defining which buttons we need to add
        foreach (var d in data)
        {
            if (!buttons.ContainsKey(d.ID))
            {
                UIOption button = createButton(d);
                list.AddChild(button);
                added.Add(new KeyValuePair<string, UIButton>(d.ID, button));
            }
        }
        
        foreach (string s in removed) { buttons.Remove(s); }

        foreach (KeyValuePair<string, UIButton> pair in added)
        {
            buttons.Add(pair.Key, pair.Value);
        }
    }
    
    EnterDataPopup popup;
    
    private void CreateMapCallback()
    {
        popup = new EnterDataPopup(Finish, "Create Map", "Map ID");
        display.ShowPopup(popup.GetRoot());
        
        string Finish(bool choice)
        {
            if (choice)
            {
                string id = popup.Get("Map ID");

                if (UTLS.ValidString(id) && env.Project.GetMap(id) == null)
                {
                    env.Project.AddMap(new MapData(id));
                }
                else
                {
                    return "Invalid ID";
                }
            }

            display.ClosePopup();
            popup = null;
            return "";
        }
    }
    private void CreateUnitCallback()
    {
        popup = new EnterDataPopup(Finish, "Create Unit", "Unit ID", "Map ID", "Override Map ID");
        display.ShowPopup(popup.GetRoot());
        
        string Finish(bool choice)
        {
            if (choice)
            {
                string id = popup.Get("Unit ID");
                string mapID = popup.Get("Map ID");
                string overrideID = popup.Get("Override Map ID");

                if (UTLS.ValidStrings(id, mapID, overrideID) && env.Project.GetUnit(id) == null)
                {
                    env.Project.AddUnit(new UnitData(id, mapID, overrideID));
                }
                else
                {
                    string msg = "";
                    
                    if (!UTLS.ValidString(id))           msg += "Invalid Unit ID";
                    if (env.Project.GetUnit(id) != null) msg += "\nUnit with the same ID already exists";
                    if (!UTLS.ValidString(mapID))        msg += "\nInvalid Map ID";
                    if (!UTLS.ValidString(overrideID))   msg += "\nInvalid Override ID";
                    
                    return msg;
                }
            }

            display.ClosePopup();
            popup = null;
            return "";
        }
    }

    private UIOption Create(MapData mapData) => new(App.UIHost, mapData.ID, () => env.BufferSwitcher.SwitchTo(mapData.ID));
    private UIOption Create(UnitData unitData) => new(App.UIHost, unitData.ID, () => env.BufferSwitcher.SwitchTo(unitData.ID));
}