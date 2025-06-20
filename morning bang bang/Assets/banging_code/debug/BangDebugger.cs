using MothDIed.InputsHandling;
using UnityEngine;

namespace banging_code.debug
{
    public sealed class BangDebugger
    {
        public static class Flags
        {
            public static bool ShowPaths = false;
        }
        
        private readonly DebuggerConfig debuggerConfig;
        
        public BangingConsole Console { get; private set; }
        public DebugLinesDrawer Lines { get; private set; }
        public DebugMapDrawer Map { get; private set; }

        private Transform container;

        public BangDebugger(DebuggerConfig config)
        {
            debuggerConfig = config;
        }

        public Transform GetDebugGOContainer()
        {
            if (container == null)
            {
                container = new GameObject("[DEBUG GAME OBJECT CONTAINER]").transform;
            }

            return container;
        }

        public void SetupDebugger()
        {
            Lines = new DebugLinesDrawer(debuggerConfig, this);
            Lines.Setup();

            Console = new BangingConsole();
            Console.Setup();

            Console.CreatePersistentConsoleView();
            
            InputService.EnableDebugInputs();
        }
    }
}