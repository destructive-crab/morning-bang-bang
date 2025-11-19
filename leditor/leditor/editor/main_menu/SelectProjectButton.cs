using deGUISpace;

namespace leditor.root;

public sealed class SelectProjectButton : GUIButton
{
    public string SelectedProject { get; private set; } = String.Empty;
    
    public SelectProjectButton(string label, RectGUIArea guiArea) : base(label, guiArea)
    {
    }

    public override void LeftMouseButtonPress()
    {
       Color = PressedColor;
    }
    
    public override void OnLeftClick()
    {
        base.OnLeftClick();

        string path = UTLS.ShowOpenProjectDialog();
        if (path != string.Empty)
        {
            SelectedProject = path;
        }
    }
}