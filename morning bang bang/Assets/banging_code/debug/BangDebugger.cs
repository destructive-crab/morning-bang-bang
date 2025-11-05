using System;
using Cysharp.Threading.Tasks;
using MothDIed;
using MothDIed.Debug;
using MothDIed.InputsHandling;
using UnityEngine;

namespace banging_code.debug
{
    public sealed class BangDebugger : IGMModuleBoot, IGMModuleTick
    {
        public static class Flags
        {
            public static bool ShowPaths = false;
            public static bool ShowFPS = false;
        }
        
        public bool Awake { get; private set; }
        
        private readonly DebuggerConfig debuggerConfig;
        
        public DebugUIRoot DebugUIRoot { get; private set; }
        
        public BangingConsole Console { get; private set; }
        public DebugLinesDrawer Lines { get; private set; }
        public DebugMapDrawer Map { get; private set; }
        public FPS FPS { get; private set; }

        private Transform container;

        public BangDebugger(DebuggerConfig config)
        {
            debuggerConfig = config;
        }

        public UniTask Boot() => SetupDebugger();

        public void Tick()
        {
            if (BangDebugger.Flags.ShowFPS)
            {
                if (FPS == null)
                {
                    FPS = new FPS(DebugUIRoot.FPSText);
                }
                
                FPS.Tick();
            }
            else if(FPS != null && !FPS.Hidden)
            {
                FPS.Hide();
            }
        }

        public Transform GetDebugGOContainer()
        {
            if (container == null)
            {
                container = new GameObject("[DEBUG GAME OBJECT CONTAINER]").transform;
                Game.G<SceneSwitcher>().MoveToPersistentScene(container.gameObject);
            }

            return container;
        }

        public async UniTask SetupDebugger()
        {
            await InstantiatePersistentDebugUI();
            
            Lines = new DebugLinesDrawer(debuggerConfig, this);
            await Lines.Setup();

            Map = new DebugMapDrawer();
            Map.Setup(debuggerConfig);

            Console = new BangingConsole();
            Console.Setup(DebugUIRoot.ConsoleView);
            
            LogHistory.OnNew += MothDIedLogHistory;
            
            InputService.EnableDebugInputs();
            Awake = true;
        }

        private void MothDIedLogHistory(LogHistory.Log logData)
        {
            switch (logData.Type)
            {
                case LogHistory.LogType.Message:
                    LGR.PM(logData.Content);
                    break;
                case LogHistory.LogType.Warning:
                    LGR.PW(logData.Content);
                    break;
                case LogHistory.LogType.Error:
                    LGR.PERR(logData.Content);
                    break;
            }
        }

        private async UniTask InstantiatePersistentDebugUI()
        {
             ResourceRequest loadOperation = Resources.LoadAsync<DebugUIRoot>("Debug/DebugUIRoot");
             await loadOperation;
             var uiPrefab = loadOperation.asset as DebugUIRoot;
             
             AsyncInstantiateOperation<DebugUIRoot> instantiateOperation = GameObject.InstantiateAsync(uiPrefab);
             await instantiateOperation;
             DebugUIRoot = instantiateOperation.Result[0];
             
             Game.G<SceneSwitcher>().MoveToPersistentScene(DebugUIRoot.gameObject);           
        }
    }
}