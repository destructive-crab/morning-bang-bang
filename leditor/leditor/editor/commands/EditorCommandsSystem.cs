using deGUISpace;
using SFML.Graphics;

namespace leditor.actions;  

public class EditorCommandsSystem
{
    public EditorCommandsContainer Container => Invoker.Container;
    public readonly CommandsInvoker Invoker;

    private GUIGroup menu;

    public void BuildGUI()
    {
        menu = new GUIGroup(new RectGUIArea(Anchor.Center, 0, 0, -1, -1));
        
        GUIRectangle background = new GUIRectangle(new Color(29, 59, 53), 20, new Color(37, 77, 69) );
        background.GUIArea = new RectGUIArea(Anchor.Center, 0, 0, 400, 200);
        
        GUIButton button = new GUIButton("_");
        button.ApplyArea(new RectGUIArea(Anchor.LeftTop, 0, 40, -1, 60));
        
        menu.AddChild(background);
        background.AddChild(button);
        
        deGUI.PushGUIElement(menu);
        
        HideInvokeMenu();
    }
    
    public void OpenInvokeMenu()
    {
       menu.Show(); 
    }

    public void HideInvokeMenu()
    {
       menu.Hide(); 
    }
    
    public EditorCommandsSystem()
    {
        Invoker = new CommandsInvoker();
    }
}