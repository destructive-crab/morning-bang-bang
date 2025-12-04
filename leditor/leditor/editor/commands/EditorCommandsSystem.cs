namespace leditor.actions;  

public class EditorCommandsSystem
{
    public EditorCommandsContainer Container => Invoker.Container;
    public readonly CommandsInvoker Invoker;


    public void BuildGUI()
    {

        HideInvokeMenu();
    }
    
    public void OpenInvokeMenu()
    {
    }

    public void HideInvokeMenu()
    {
    }
    
    public EditorCommandsSystem()
    {
        Invoker = new CommandsInvoker();
    }
}