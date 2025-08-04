using System;
using banging_code;
using Cysharp.Threading.Tasks;
using MothDIed.Scenes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MothDIed.Core.GameObjects.Pool
{
    public class PoolFabric : IFabric
    {
        public bool AvoidSceneFabric; 
        //if true, objects will be directly instantiated and destroyed with GameObject.Instantiate.
        //if false, Game.CurrentScene.Fabric will be used
        //if there is no scene in game, objects cannot be populated
        public bool AutoInject;
        //auto inject module analog
        //if scene has own auto inject -> inject in this fabric will be skipped

        public PoolFabric(bool avoidSceneFabric, bool autoInject)
        {
            AvoidSceneFabric = avoidSceneFabric;
            AutoInject = autoInject;
        }

        public PoolFabric SetInstantiateParameters(bool avoidSceneFabric, bool autoInject)
        {
            AvoidSceneFabric = avoidSceneFabric;
            AutoInject = autoInject;
            
            return this;
        }

        public UniTask<TObject> InstantiateAsync<TObject>(TObject original, Action<TObject> callback = null) where TObject : Object => InstantiateAsync(original, Vector3.zero, Quaternion.identity, null, callback);
        public UniTask<TObject> InstantiateAsync<TObject>(TObject original, Transform parent, Action<TObject> callback = null) where TObject : Object => InstantiateAsync(original, Vector3.zero, Quaternion.identity, parent, callback);
        public UniTask<TObject> InstantiateAsync<TObject>(TObject original, Vector3 position, Action<TObject> callback = null) where TObject : Object => InstantiateAsync(original, position, Quaternion.identity, null, callback);
        public UniTask<TObject> InstantiateAsync<TObject>(TObject original, Vector3 position, Transform parent, Action<TObject> callback = null) where TObject : Object => InstantiateAsync(original, position, Quaternion.identity, parent, callback);
        public UniTask<TObject> InstantiateAsync<TObject>(TObject original, Vector3 position, Quaternion rotation, Action<TObject> callback = null) where TObject : Object => InstantiateAsync(original, position, rotation, null, callback);
        public virtual async UniTask<TObject> InstantiateAsync<TObject>(TObject original, Vector3 position, Quaternion rotation, Transform parent,
            Action<TObject> callback = null) where TObject : Object
        {
            TObject instance;
            
            if (AvoidSceneFabric)
            { 
                AsyncInstantiateOperation<TObject> instantiateOperation = GameObject.InstantiateAsync(original, parent, position, rotation);
                
                await instantiateOperation;
                
                instance = instantiateOperation.Result[0];

                if (AutoInject)
                {
                    Game.DIKernel.InjectWithBase(instance);
                }
            }
            else
            {
                if (Game.SceneSwitcher.IsSceneLoaded)
                {
                    UniTask<TObject> instantiateTask = Game.SceneSwitcher.CurrentScene.Fabric.InstantiateAsync(original, position, rotation, parent);
                    await instantiateTask;
                    instance = await instantiateTask;
                    
                    if (AutoInject && !Game.SceneSwitcher.CurrentScene.Modules.Contains<FabricAutoInjectModule>())
                    {
                        Game.DIKernel.InjectWithBase(instance);
                    }
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogError("[POOL FABRIC : INSTANTIATE] TRYING TO USE POOL FABRIC WHEN SCENE IS NOT LOADED");
#endif
                    return null;
                }
            }

            callback?.Invoke(instance);
            return instance;
        }

        public TObject Instantiate<TObject>(TObject original) where TObject : Object => Instantiate(original, Vector3.zero, Quaternion.identity, null);
        public TObject Instantiate<TObject>(TObject original, Vector3 position) where TObject : Object => Instantiate(original, position, Quaternion.identity, null);
        public TObject Instantiate<TObject>(TObject original, Transform parent) where TObject : Object => Instantiate(original, Vector3.zero, Quaternion.identity, parent);
        public TObject Instantiate<TObject>(TObject original, Vector3 position, Transform parent) where TObject : Object => Instantiate(original, position, Quaternion.identity, parent);
        public TObject Instantiate<TObject>(TObject original, Vector3 position, Quaternion rotation) where TObject : Object => Instantiate(original, position, rotation, null);

        public virtual TObject Instantiate<TObject>(TObject original, Vector3 position, Quaternion rotation, Transform parent) where TObject : Object
        {
            TObject instance;
            
            if (AvoidSceneFabric)
            { 
                instance = GameObject.Instantiate(original, position, rotation, parent);

                if (AutoInject)
                {
                    Game.DIKernel.InjectWithBase(instance);
                }
            }
            else
            {
                if (Game.SceneSwitcher.IsSceneLoaded)
                {
                    instance = Game.SceneSwitcher.CurrentScene.Fabric.Instantiate(original, position, rotation, parent);
                    
                    if (AutoInject && !Game.SceneSwitcher.CurrentScene.Modules.Contains<FabricAutoInjectModule>())
                    {
                        Game.DIKernel.InjectWithBase(instance);
                    }
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogError("[POOL FABRIC : INSTANTIATE] TRYING TO USE POOL FABRIC WHEN SCENE IS NOT LOADED");
#endif
                    return null;
                }
            }

            return instance;
        }

        public virtual void Destroy(Object toDestroy)
        {
            if (AvoidSceneFabric)
            {
                GameObject.Destroy(toDestroy);
            }
            else if (Game.SceneSwitcher.IsSceneLoaded)
            {
                Game.SceneSwitcher.CurrentScene.Fabric.Destroy(toDestroy);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("[POOL FABRIC : DESTROY] TRYING TO USE POOL FABRIC WHEN SCENE IS NOT LOADED");
#endif
            }
        }
    }
}