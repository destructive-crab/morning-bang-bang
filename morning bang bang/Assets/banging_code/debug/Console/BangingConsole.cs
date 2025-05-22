using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using banging_code.debug.Console;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BangingConsole : MonoBehaviour
{
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private GameObject root;

    public static BangingConsole Instance { get; private set; }
    private ConsoleOutput output;
    private ConsoleInput input;
    private ConsoleState state;

    private readonly Dictionary<string, ConsoleCommand> commands = new();

    public ConsoleCommandsContainer Container { get; private set; }
    public ConsoleCommand[] CommandList => commands.Values.ToArray();

    private void Awake()
    {
        MonoSingletonLogic();
        SetupCommands();
        SetupInputAndOutput();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            switch (state)
            {
                case ConsoleState.Focused:
                    state = ConsoleState.Unfocused;
                    input.Disable();
                    break;
                case ConsoleState.Unfocused:
                    state = ConsoleState.Disabled;
                    root.SetActive(false);
                    break;
                case ConsoleState.Disabled:
                    state = ConsoleState.Focused;
                    root.SetActive(true);
                    input.Enable();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        if (state == ConsoleState.Focused && Input.GetKeyDown(KeyCode.Return))
        {
            var command = input.GetParsed();

            if (command[0] is not string) return;

            if (commands.ContainsKey(command[0] as string))
            {
                commands[command[0] as string].TryInvoke(command.Skip(1).ToArray());
            }
        }
    }

    public void InvokeCommand(string key, params object[] args)
    {
        commands[key].TryInvoke(args);
    }

    public void ClearOutput()
    {
        output.Clear();
    }

    public void PushMessage(string message)
    {
        output.PushFromNewLine("[" + System.DateTime.Now.ToLongTimeString() + "] " + message);
    }

    public void PushError(string message)
    {
        output.PushFromNewLine("! [" + System.DateTime.Now.ToLongTimeString() + "] " + message + " !");
    }

    private void SetupInputAndOutput()
    {
        output = new ConsoleOutput(outputText);
        input = new ConsoleInput(GetComponentInChildren<TMP_InputField>());
    }

    private void SetupCommands()
    {
        var allCommands = typeof(ConsoleCommandsContainer).GetMethods(BindingFlags.Public | BindingFlags.Instance);

        foreach (MethodInfo commandBase in allCommands)
        {
            if(!commandBase.HasAttribute(typeof(ConsoleCommandKeyAttribute))) return;
            
            ConsoleCommand command = new ConsoleCommand(commandBase);
            commands.Add(command.Key, command);
        }
    }

    private void MonoSingletonLogic()
    {
        if (Instance == null)
        {
            Instance = this;
            Container = new ConsoleCommandsContainer();
            DontDestroyOnLoad(gameObject);
            
            return;
        }

        Destroy(gameObject);
    }

    enum ConsoleState
    {
        Focused,
        Unfocused,
        Disabled
    }
}
