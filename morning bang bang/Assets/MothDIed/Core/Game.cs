using Cysharp.Threading.Tasks;
using MothDIed.Debug;
using MothDIed.ServiceLocators;
using UnityEngine;

namespace MothDIed
{
    public static class Game
    {
        public static bool AllowDebug { get; private set; }
        //STATE
        public static bool Awake { get; private set; } = false;
        public static bool IsBootstrapping { get; private set; } = true;

        private static readonly GMModulesStorage modulesStorage = new();
        
        public static async UniTask StartGame(bool allowDebug, GameStartPoint point)
        {
            AllowDebug = allowDebug;
            
            point.BuildModules(modulesStorage);

            string debugMessage = "";
            
            foreach (IGMModuleBoot boot in modulesStorage.boots)
            {
                await boot.Boot();
                debugMessage += $"Module {boot.ToString()} booted" + boot.ToString() + "\n";
            }

            if (AllowDebug)
            {
                string bootModulesLog = debugMessage;
                debugMessage = $"Game started with from {point.GetType().ToString()}({point.name}). ";
                debugMessage += $"With {modulesStorage.Count} modules, including: ";

                foreach (object module in modulesStorage.all)
                {
                    debugMessage += module.ToString() + "; ";
                }
            }
            
            
            LogHistory.PushAsMessage(debugMessage);
            
            IsBootstrapping = false;
            Awake = true;
            
            point.Complete();
            
            InnerLoop();
        }

        public static async void QuitGame()
        { 
            Awake = false;
            
            foreach (IGMModuleQuit quit in modulesStorage.quits)
            {
                await quit.Quit();
            }
            
            Application.Quit();
        }
        
        private static async void InnerLoop()
        {
            while (Awake)
            {
                if (G<SceneSwitcher>().CurrentScene != null && G<SceneSwitcher>().IsSceneLoaded)
                {
                    foreach (IGMModuleTick tick in modulesStorage.ticks)
                    {
                        tick.Tick();
                    }
                    
                    G<SceneSwitcher>().CurrentScene?.UpdateScene();
                }

                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

                while (!Application.isPlaying)
                {
                    await UniTask.Yield();
                }
            }
        }

        public static TModule G<TModule>() 
            where TModule : class
        {
            return modulesStorage.Get<TModule>();
        }
        
        public static bool TG<TModule>(out TModule module)
            where TModule : class
        {
            module = modulesStorage.Get<TModule>();
            return module != null;
        }

        public static IServiceLocator GetModulesLocator()
        {
            return modulesStorage;
        }
    }

    public interface IGMModuleBoot
    {
        public UniTask Boot();
    }

    public interface IGMModuleTick
    {
        public void Tick();
    }

    public interface IGMModuleQuit
    {
        public UniTask Quit();
    }
}