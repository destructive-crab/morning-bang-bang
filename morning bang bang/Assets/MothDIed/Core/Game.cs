using Cysharp.Threading.Tasks;
using MothDIed.DI;
using UnityEngine;
using System;
using banging_code.pause;
using banging_code.runs_system;
using MothDIed.InputsHandling;
using MothDIed.Scenes;

namespace MothDIed
{
    public static class Game
    {
        //scene management
        public static bool IsSceneLoaded { get; private set; }
        public static Scene CurrentScene { get; private set; }

        //callbacks
        public static event Action<Scene> OnSwitchingFromCurrent; //prev
        public static event Action<Scene, Scene> OnSwitched; //prev/new
        public static event Action<Scene> OnLoaded; //new

        //other
        public static readonly RunSystem RunSystem = new();
        public static readonly PauseSystem PauseSystem = new();
        
        //core
        public static readonly DIKernel DIKernel = new();
        private static readonly AsyncSceneLoader Loader = new();

        public static void SwitchTo<TScene>(TScene scene, Action onSwitched = null)
            where TScene : Scene
        {
            OnSwitchingFromCurrent?.Invoke(CurrentScene);
            
            IsSceneLoaded = false;
            
            //clearing previous scene if exists
            if (CurrentScene != null)
            {
                CurrentScene?.DisposeScene();
                DIKernel.ClearSceneDependenciesContainer();
            }
            
            //starting loading new scene
            CurrentScene = scene;

            scene.InitModules();
            scene.PrepareScene();
            
            DIKernel.RegisterSceneDependencies(scene);
            
            Loader.LoadScene(scene.GetSceneName(), Complete);
            
            return;

            void Complete() 
            {
                Scene prevScene = CurrentScene;
    
                scene.LoadScene();
    
                IsSceneLoaded = true;
                
                onSwitched?.Invoke();
                OnSwitched?.Invoke(prevScene, scene);
                OnLoaded?.Invoke(scene);
            }
        }

        public static async void InnerLoop()
        {
            while (true)
            {
                if (CurrentScene != null && IsSceneLoaded)
                {
                    CurrentScene?.UpdateScene();

                    InputService.Tick();
                    EventManager.Tick();

                    if (RunSystem.IsInRun && RunSystem.Data.Level != null && IsSceneLoaded)
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
}