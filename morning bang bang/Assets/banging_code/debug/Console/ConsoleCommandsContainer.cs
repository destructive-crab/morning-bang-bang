using MothDIed;

namespace banging_code.debug.Console
{
    public sealed class ConsoleCommandsContainer
    {
        [ConsoleCommandKey("all")]
        [ConsoleCommandDescription("Prints all commands in console")]
        public void All()
        {
            foreach (var command in BangingConsole.Instance.CommandList)
            {
                BangingConsole.Instance.InvokeCommand(command.Key, "help");
            }
        }
                
        [ConsoleCommandKey("clear")] 
        [ConsoleCommandDescription("Clears console output")]
        public void Clear()
        {
            BangingConsole.Instance.ClearOutput();
        } 
        
        [ConsoleCommandKey("echo")] 
        [ConsoleCommandDescription("Prints given text in console")]
        public void Echo(string text)
        {
            BangingConsole.Instance.PushMessage(text);
        }      
        
        [ConsoleCommandKey("show_paths")] 
        [ConsoleCommandDescription("Is debug path's visualisation enabled")]
        public void ShowPaths(bool enabled)
        {
            Game.DebugFlags.ShowPaths = enabled;
            BangingConsole.Instance.PushMessage($"Show Debug Paths: {enabled}");
        }      
    }
}