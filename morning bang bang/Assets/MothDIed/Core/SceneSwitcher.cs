using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = MothDIed.Scenes.Scene;

namespace MothDIed
{
    public class SceneSwitcher
    {
        public bool IsSceneLoaded { get; private set; }
        public bool IsPersistentSceneLoaded { get; private set; }
        
        public Scene CurrentScene { get; private set; }
        
        public event Action<Scene> OnSwitchingFromCurrent; //prev
        public event Action<Scene, Scene> OnSwitched; //prev/new
        public static event Action<Scene> OnLoaded; //new

        private static SceneSwitcher sceneSwitcher;
        private static UnityEngine.SceneManagement.Scene persistentScene;
        
        public SceneSwitcher()
        {
            if (sceneSwitcher == null)
            {
                sceneSwitcher = this;
            }
            else
            {
                throw new Exception("MULTIPLE SCENE SWITCHER INSTANCES ARE NOT ALLOWED");
            }
        }

        public async UniTaskVoid LoadPersistentScene()
        {
            persistentScene = SceneManager.CreateScene("PERSISTENT SCENE");
            SceneManager.LoadScene(persistentScene.name);
        }

        public async UniTaskVoid SwitchTo<TScene>(TScene scene, Action onSwitched = null)
            where TScene : Scene
        {
            OnSwitchingFromCurrent?.Invoke(CurrentScene);
            IsSceneLoaded = false;
            
            //clearing previous scene if exists
            if (CurrentScene != null)
            {
                CurrentScene?.DisposeScene();
                Game.DIKernel.ClearSceneDependenciesContainer();

                await SceneManager.UnloadSceneAsync(CurrentScene.GetSceneName());
            }

            if (SceneManager.sceneCount > 1)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var loadedScene = SceneManager.GetSceneAt(i);
                    
                    if (loadedScene.name != "PERSISTENT SCENE")
                    {
#if UNITY_EDITOR
#endif
                        await SceneManager.UnloadSceneAsync(loadedScene.name);
                    }
                }
            }
            
            //starting loading new scene
            CurrentScene = scene;

            scene.InitModules();
            scene.PrepareScene();

            Game.DIKernel.RegisterSceneDependencies(scene);
            
            LoadScene(scene.GetSceneName(), Complete);
            
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
        
        public void LoadScene(string sceneName, Action onSceneLoadedCallback = null)
        {
            if (GetCurrentSceneName() == sceneName)
            {
                onSceneLoadedCallback?.Invoke();
                return;
            }

            var loadSceneOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            loadSceneOperation.completed += (_) => onSceneLoadedCallback?.Invoke();
        }

        
        private static string GetCurrentSceneName() => SceneManager.GetActiveScene().name;
    }
}