using banging_code.debug;
using Cysharp.Threading.Tasks;
using MothDIed.DI;
using UnityEngine;
using banging_code.pause;
using banging_code.runs_system;
using MothDIed.InputsHandling;

namespace MothDIed
{
    public static class Game
    {
        //STATE
        public static bool Awake { get; private set; } = false;
        
        //SERVICES 
        //core
        public static readonly SceneSwitcher SceneSwitcher = new();
        public static readonly DIKernel DIKernel = new();
        
        //debug
        public static BangDebugger GetDebugger()
        {
            if (Awake && AllowDebug)
            {
                return debugger;
            }

            return null;
        }

        public static bool TryGetDebugger(out BangDebugger debugger)
        {
            debugger = Game.debugger;
            return Awake && AllowDebug;
        }

        public static bool AllowDebug { get; private set; }
        private static BangDebugger debugger;
        
        //other
        public static readonly RunSystem RunSystem = new();
        public static readonly PauseSystem PauseSystem = new();

        public static void StartGame(GameStartArgs args)
        {
            InputService.Initialize();

            if (args.AllowDebug)
            {
                debugger = new BangDebugger();
            }
            
            Awake = true;
            InnerLoop();
        }
        
        private static async void InnerLoop()
        {
            while (Awake)
            {
                if (SceneSwitcher.CurrentScene != null && SceneSwitcher.IsSceneLoaded)
                {
                    SceneSwitcher.CurrentScene?.UpdateScene();

                    InputService.Tick();
                    EventManager.Tick();

                    if (RunSystem.IsInRun && RunSystem.Data.Level != null && SceneSwitcher.IsSceneLoaded)
                    {
                        RunSystem.WhileOnLevel(RunSystem.Data.Level);
                    }
                }

                await UniTask.WaitForFixedUpdate();

                while (!Application.isPlaying)
                {
                    await UniTask.Yield();
                }
            }
        }

        public static class DebugFlags
        {
            public static bool ShowPaths = false;
        }
    }

    public class GameStartArgs
    {
        public readonly bool AllowDebug;

        public GameStartArgs(bool allowDebug)
        {
            AllowDebug = allowDebug;
        }
    }
}