using leditor.UI;
using SFML.System;

namespace leditor.root;

public abstract class EditorDisplay
{
    public abstract UIHost Host { get; }
    public virtual void Tick() {}
}
public sealed class HomeDisplay : EditorDisplay
{
    public override UIHost Host => host;
    
    public bool Hidden { get; private set; } = true;
    
    private UIHost host;

    public HomeDisplay()
    {
        host = new UIHost(new UIStyle());
        
        host.Root = new AxisBox(host, UIAxis.Vertical,
            new UILimit(host, new Vector2f(100, 50)),
            new UILabel(host, "Home (-) . (-)"),
            new UIButton(host, "Load Project", LoadProjectButton),
            new UIButton(host, "Create New Project", NewProjectButton));
        return;
        host.Root = new AxisBox(host, UIAxis.Vertical, [
            new UILabel(host, "Texture ID"),
            new UIEntry(host, new UIVar<string>("id here")),
            new UILabel(host, "Texture Path"),
            new AxisBox(host, UIAxis.Horizontal, new UIEntry(host, new UIVar<string>("id hesdadasdadasdadadadasdadsadre"), 215), new UIButton(host, "Choose")),           
            new UILabel(host, "Texture Start"),
            new AxisBox(host, UIAxis.Horizontal, new UILabel(host, "X"), new UIEntry(host, new UIVar<string>("100"), 120), new UILabel(host, "Y"), new UIEntry(host, new UIVar<string>("100"), 120)),           
            new UILabel(host, "Texture Size"),
            new AxisBox(host, UIAxis.Horizontal, new UILabel(host, "X"), new UIEntry(host, new UIVar<string>("100"), 120), new UILabel(host, "Y"), new UIEntry(host, new UIVar<string>("100"), 120)),           
        ]);

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