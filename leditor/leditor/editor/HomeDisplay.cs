using leditor.UI;
using SFML.System;

namespace leditor.root;

public sealed class HomeDisplay : EditorDisplay
{
    public override UIHost Host => host;
    
    public bool Hidden { get; private set; } = true;
    
    private UIHost host;

    public HomeDisplay()
    {
        host = new UIHost(new UIStyle(), new Vector2f(App.WindowHandler.Width, App.WindowHandler.Height));
        
        host.SetRoot(new AxisBox(host, UIAxis.Vertical,
            new UILimit(host, new Vector2f(100, 50)),
            new UILabel(host, "Home (-) . (-)"),
            new UIButton(host, "Load Project", LoadProjectButton),
            new UIButton(host, "Create New Project", NewProjectButton)));
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