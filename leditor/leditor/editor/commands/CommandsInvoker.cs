using System.Reflection;
using leditor.actions;

public class CommandsInvoker 
{
    //commands
    private readonly Dictionary<string, EditorCommand> commands = new();
    public EditorCommand[] CommandList => commands.Values.ToArray();
    public readonly EditorCommandsContainer Container = new();
    
    //output
    private readonly List<string> outputHistory = new();
    
    public string[] OutputHistory => outputHistory.ToArray();

    public CommandsInvoker()
    {
        CollectCommandFromContainer();
    }

    private void CollectCommandFromContainer()
    {
        MethodInfo[] allCommands = typeof(EditorCommandsContainer).GetMethods(BindingFlags.Public | BindingFlags.Instance);

        foreach (MethodInfo commandBase in allCommands)
        {
            if(!commandBase.GetCustomAttributes(typeof(CommandBaseAttribute), false).Any()) return;
            
            EditorCommand command = new EditorCommand(commandBase);
            commands.Add(command.Key, command);
        }
    }

    public bool HasCommand(string key)
    {
        return commands.ContainsKey(key);
    }

    public Type[] GetArgumentsForCommand(string key)
    {
        if (!HasCommand(key)) return null;
        return commands[key].ArgsAsType;
    }

    public string InvokeCommand(string key, params object[] args)
    {
        string output = InvokeCommandInternal(key, args);
        
        outputHistory.Add(output);

        return output;
    }
    
    private string InvokeCommandInternal(string key, object[] args)
    {
        if (!commands.ContainsKey(key))
        {
            return $"[!] Command with key {key} not found";
        }
        
        return commands[key].TryInvoke(args);
    }
}