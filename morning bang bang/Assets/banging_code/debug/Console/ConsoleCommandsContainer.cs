using MothDIed;

namespace banging_code.debug.Console
{
    public sealed class ConsoleCommandsContainer
    {
        [ConsoleCommandKey("all")]
        [ConsoleCommandDescription("Prints all commands in console")]
        public string All()
        {
            string output = "";

            foreach (ConsoleCommand consoleCommand in Game.GetDebugger().Console.CommandList)
            {
                output += "\n";
                output += consoleCommand.GetCommandPattern() + ". " + consoleCommand.Description;
            }

            return output;
        }
                
        [ConsoleCommandKey("clear")] 
        [ConsoleCommandDescription("Clears console output")]
        public string Clear()
        {
            return "";
        } 
        
        [ConsoleCommandKey("echo")] 
        [ConsoleCommandDescription("Prints given text in console")]
        public string Echo(string text)
        {
            return text;
        }      
        
        [ConsoleCommandKey("echo_error")] 
        [ConsoleCommandDescription("Prints given text in console")]
        public string EchoError(string text)
        {
            return "[! ERROR OCCURED] " + text + " [! ERROR OCCURED]";
        }      
        
        [ConsoleCommandKey("echo_warning")] 
        [ConsoleCommandDescription("Prints given text in console")]
        public string EchoWarning(string text)
        {
            return "[!] " + text + " [!]";
        }  
        
        [ConsoleCommandKey("show_paths")] 
        [ConsoleCommandDescription("Is debug path's visualisation enabled")]
        public string ShowPaths(bool enabled)
        {
            BangDebugger.Flags.ShowPaths = enabled;
            return "DebugPaths enabled";
        }

        [ConsoleCommandKey("draw_map")]
        [ConsoleCommandDescription("Draws map cells indicators")]
        public string DrawMap()
        {
            Game.GetDebugger().Map.DrawMap();
            return "Map drawn";
        }
        
        [ConsoleCommandKey("hide_map")]
        [ConsoleCommandDescription("Draws map cells indicators")]
        public string HideMap()
        {
            Game.GetDebugger().Map.HideMap();
            return "Map hidden";
        }
    }
}