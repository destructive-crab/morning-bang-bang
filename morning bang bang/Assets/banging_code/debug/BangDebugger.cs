using Cysharp.Threading.Tasks;
using DragonBones;
using MothDIed;
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
        
        public bool Awake { get; private set; }
        
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
                Game.SceneSwitcher.MoveToPersistentScene(container.gameObject);
            }

            return container;
        }

        public async UniTask SetupDebugger()
        {
            Lines = new DebugLinesDrawer(debuggerConfig, this);
            await Lines.Setup();

            Map = new DebugMapDrawer();
            Map.Setup(debuggerConfig);

            Console = new BangingConsole();
            Console.Setup();

            await Console.CreatePersistentConsoleView();
            
            InputService.EnableDebugInputs();
            Awake = true;
        }
    }
}