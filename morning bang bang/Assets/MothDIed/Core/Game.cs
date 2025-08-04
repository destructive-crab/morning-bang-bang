using System;
using banging_code.debug;
using banging_code.json;
using Cysharp.Threading.Tasks;
using MothDIed.DI;
using UnityEngine;
using banging_code.pause;
using banging_code.runs_system;
using banging_code.settings;
using DragonBones;
using MohDIed.Audio;
using MothDIed.Audio;
using MothDIed.InputsHandling;

namespace MothDIed
{
    public static class Game
    {
        //STATE
        public static bool Awake { get; private set; } = false;
        public static bool IsBootstrapping { get; private set; } = true;
        
        //SERVICES 
        //core
        public static readonly SceneSwitcher SceneSwitcher = new();
        public static readonly DIKernel DIKernel = new();
        public static readonly GameSettings Settings = new();
        public static readonly AudioSystem AudioSystem = new();
        
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

        public static async UniTask StartGame(GameStartArgs args)
        { 
            await Settings.LoadFromFile();
            
            InputService.Setup();
            
            await SceneSwitcher.CreatePersistentScene();

            AllowDebug = Settings.Data.EnableDebugFeatures;
            
            if (AllowDebug)
            {
                debugger = new BangDebugger(args.DebuggerConfig); 
                await debugger.SetupDebugger();
            }
            
            await DBInitial.InitializeDragonBones();
            
            AudioSystem.Setup(args.AudioSystemConfig);
            
            IsBootstrapping = false;
            Awake = true;
            
            InnerLoop();
        }

        public async static void QuitGame()
        { 
            await Settings.SaveSettings();
            await SaveManager.StartWritingFilesFromQueue();
            
            Application.Quit();
        }

        public static bool LoadSaves()
        {
            return RunSystem.SaveRun();
        }
        
        public static void MakeGameObjectPersistent(GameObject gameObject)
        {
            SceneSwitcher.MoveToPersistentScene(gameObject);
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
                    DBInitial.Kernel.AdvanceTime(Time.deltaTime);

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

    }

    [Serializable]
    public class GameStartArgs
    {
        public DebuggerConfig DebuggerConfig;
        public AudioSystemConfig AudioSystemConfig;
    }
}