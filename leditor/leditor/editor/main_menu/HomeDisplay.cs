using leditor.UI;
using SFML.System;

namespace leditor.root;

public abstract class EditorDisplay
{
    public abstract UIHost Host { get; }
}
public sealed class HomeDisplay : EditorDisplay
{
    public override UIHost Host => uiHost;
    
    public bool Hidden { get; private set; } = true;
    
    private UIHost uiHost;

    public HomeDisplay()
    {
        uiHost = new UIHost(new UIStyle());
        
        uiHost.Root = new AxisBox(uiHost, UIAxis.Vertical,
            new UILimit(uiHost, new Vector2f(100, 50)),
            new UILabel(uiHost, "Home (-) . (-)"),
            new UIButton(uiHost, "Load Project", LoadProjectButton),
            new UIButton(uiHost, "Create New Project", NewProjectButton));
        
    }

    private void LoadProjectButton()
    {
        string path = UTLS.ShowOpenProjectDialog();
        if(path != string.Empty) App.LeditorInstance.ProjectEnvironment.OpenProjectAtPath(path);
    }

    public void NewProjectButton()
    {
        App.LeditorInstance.ProjectEnvironment.OpenEmptyProject();
    }

    public void Open()
    {
        if (!Hidden) return;
        
        Hidden = false;
    }

    public void Close()
    {
        if (Hidden) return;
        
        Hidden = true;
    }
}