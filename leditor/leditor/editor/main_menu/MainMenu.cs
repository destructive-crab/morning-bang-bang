using deGUISpace;
using SFML.Graphics;

namespace leditor.root;

public sealed class MainMenu
{
    public bool Hidden { get; private set; } = true;
    public string SelectedProject => ProjectButton.SelectedProject;

    private GUIGroup group;
    private SelectProjectButton ProjectButton;

    public MainMenu()
    {
        group = new GUIGroup(new RectGUIArea(Anchor.Center, 0, 0, -1, -1));
        GUIElement background = group.AddChild(new GUIRectangle(new Color(19, 38, 35), 0, Color.Black, new RectGUIArea(Anchor.LeftTop, 0, 0, -1, -1)));
        ProjectButton = new SelectProjectButton("Choose Project", new RectGUIArea(Anchor.Center, 0, 0, 700, 200));
        background.AddChild(ProjectButton);
        group.Hide();
    }

    public void Open()
    {
        if (!Hidden) return;
        deGUI.PushGUIElement(group);
        group.Show();
        Hidden = false;
    }

    public void Close()
    {
        if (Hidden) return;
        deGUI.RemoveGUIElement(group);
        group.Hide();
        Hidden = true;
    }
}