using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using banging_code.debug.Console;
using Cysharp.Threading.Tasks;
using MothDIed;
using Unity.VisualScripting;
using UnityEngine;

public class BangingConsole 
{
    //commands
    private readonly Dictionary<string, ConsoleCommand> commands = new();
    public ConsoleCommand[] CommandList => commands.Values.ToArray();
    public readonly ConsoleCommandsContainer Container = new();
    
    //output
    private readonly List<string> outputHistory = new();
    
    public ConsoleView View { get; private set; }
    public string[] OutputHistory => outputHistory.ToArray();

    public void Setup()
    {
        CollectCommandFromContainer();
    }

    public async UniTask CreatePersistentConsoleView()
    { 
        ResourceRequest loadOperation = Resources.LoadAsync<ConsoleView>("Debug/Console View");
        await loadOperation;
        ConsoleView viewPrefab = loadOperation.asset as ConsoleView;
        
        AsyncInstantiateOperation<ConsoleView> instantiateOperation = GameObject.InstantiateAsync(viewPrefab);
        await instantiateOperation;
        ConsoleView viewInstance = instantiateOperation.Result[0];
        
        Game.MakeGameObjectPersistent(viewInstance.gameObject);
        View = viewInstance;
        View.Disable();
    }
    
    private void CollectCommandFromContainer()
    {
        var allCommands = typeof(ConsoleCommandsContainer).GetMethods(BindingFlags.Public | BindingFlags.Instance);

        foreach (MethodInfo commandBase in allCommands)
        {
            if(!commandBase.HasAttribute(typeof(ConsoleCommandKeyAttribute))) return;
            
            ConsoleCommand command = new ConsoleCommand(commandBase);
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

    public string InvokeCommand(bool printOutputToConsole, string key, params object[] args)
    {
        string output = InvokeCommandInternal(key, args);

        if (printOutputToConsole && View != null)
        {
            View.PushNewMessage(output);
        }
        
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
