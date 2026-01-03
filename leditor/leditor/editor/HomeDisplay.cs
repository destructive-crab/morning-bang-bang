using leditor.root.UI.Serializer;
using leditor.UI;
using SFML.System;

namespace leditor.root;

public sealed class HomeDisplay : EditorDisplay
{
    public override UIHost Host => h;
    
    public bool Hidden { get; private set; } = true;
    
    private UIHost h;

    public HomeDisplay()
    {
        h = App.UIHost;
        
        h.SetRoot(new AxisBox(h, UIAxis.Vertical,
            new UILabel(h, "Home (-) . (-)"),
            new UIButton(h, "Load Project", LoadProjectButton),
            new UIButton(h, "Create New Project", NewProjectButton),
            new PathEntry("FOLDER", "a"),
            new UI.Serializer.TextureReference("TEXTURE", "test"),
            new UIIntVecEntry("Test", 100, 100, 800)));

        return;
        var t = RuntimeAssetsStorage.GetInvalidTexture();
        var r = new Rect(0, 0, 100, 100);
        
        h.SetRoot(new AxisBox(h, UIAxis.Vertical,
    [ new AxisBox(h, UIAxis.Horizontal, new UIImageButton(h, t, r, new Vector2f(1,1)), new UIImageButton(h, t, r, new Vector2f(1,1)), new UIImageButton(h, t, r, new Vector2f(1,1))), 
    new AxisBox(h, UIAxis.Horizontal, new UIImageButton(h, t, r, new Vector2f(1,1)), new UIImageButton(h, t, r, new Vector2f(1,1)), new UIImageButton(h, t, r, new Vector2f(1,1))), 
    new AxisBox(h, UIAxis.Horizontal, new UIImageButton(h, t, r, new Vector2f(1,1)), new UIImageButton(h, t, r, new Vector2f(1,1)), new UIImageButton(h, t, r, new Vector2f(1,1))), ]));
        
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