using System;
using MothDIed.Core.GameObjects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MothDIed.Scenes
{
    public class GameFabric
    {
        public event Action<Object> OnInstantiated;
        public event Action<Object> OnDestroyed;
        
        public event Action<GameObject> OnGameObjectInstantiated;
        public event Action<GameObject> OnGameObjectDestroyed;

        private GameFabricSceneModule[] modules;

        public void RefreshModules()
        {
            modules = Game.CurrentScene.Modules.GetAllOfType<GameFabricSceneModule>();
        }
        
        public virtual TObject Instantiate<TObject>(TObject original, Vector3 position)
            where TObject : Object
        {
            return Instantiate(original, position, Quaternion.identity, null);
        }

        public virtual TObject Instantiate<TObject>(TObject original, Vector3 position, Transform parent)
            where TObject : Object
        {
            return Instantiate(original, position, Quaternion.identity, parent);
        }

        public virtual TObject Instantiate<TObject>(TObject original, Vector3 position, Quaternion rotation)
            where TObject : MonoBehaviour
        {
            return Instantiate(original, position, rotation, null);
        }

        public virtual TObject Instantiate<TObject>(TObject original, Vector3 position, Quaternion rotation,
            Transform parent)
            where TObject : Object
        {
            var instance = Object.Instantiate(original, position, rotation, parent);

            foreach (var module in modules)
            {
                module.OnInstantiated(instance);
            }

            OnInstantiated?.Invoke(instance);
            InvokeGameObjectEventsIfGameObject();

            return instance;

            void InvokeGameObjectEventsIfGameObject()
            {
                if (instance is GameObject gameObject)
                {
                    OnGameObjectInstantiated?.Invoke(gameObject);
                }
                else if (instance is Component component)
                {
                    OnGameObjectInstantiated?.Invoke(component.gameObject);
                }
            }
        }

        public virtual void Destroy(Object toDestroy)
        {
            OnDestroyed?.Invoke(toDestroy);
            
            if(toDestroy is GameObject gameObject)
            {
                OnGameObjectDestroyed?.Invoke(gameObject);
            }
            
            Object.Destroy(toDestroy);
        }
    }
}