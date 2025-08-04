using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MothDIed.Scenes;
using UnityEngine;

namespace MothDIed.Core.GameObjects.Pool
{
    public class GameObjectPool<TObject> : IDisposable
        where TObject : Component
    {
        public Config<TObject> PoolConfiguration{ get; private set; }
        
        private readonly List<TObject> Available = new();
        private readonly List<TObject> CurrentlyInUse = new();

        public PoolInstanceRoot Root { get; private set; }

        public bool IsPoolReady { get; private set; } = false;
        public int TotalCount => Available.Count + CurrentlyInUse.Count;
        public ushort CurrentSize { get; private set; }

        public GameObjectPool(Config<TObject> poolConfiguration)
        {
            PoolConfiguration = poolConfiguration;
            CurrentSize = PoolConfiguration.Size;
        }

        public GameObjectPool(TObject prefab) : this(new Config<TObject>(prefab)) { }

        public void Dispose()
        {
            PoolConfiguration.Fabric.Destroy(Root.gameObject);
        }

        public GameObjectPool<TObject> Warm()
        {
            if (IsPoolReady) return this;
            
            Root = new GameObject().AddComponent<PoolInstanceRoot>();
            Root.Setup(PoolConfiguration.Name);
            
            if(PoolConfiguration.Persistent)
            {
                Game.MakeGameObjectPersistent(Root.gameObject);
            }

            TryPopulateUntilPoolWillBeFull();
            
            IsPoolReady = true;
            return this;
        }

        
        public async UniTask<GameObjectPool<TObject>> WarmAsync()
        {
            if (IsPoolReady) return this;
            
            Root = new GameObject().AddComponent<PoolInstanceRoot>();
            Root.Setup(PoolConfiguration.Name);
            
            if(PoolConfiguration.Persistent)
            {
                Game.MakeGameObjectPersistent(Root.gameObject);
            }

            await TryPopulateUntilPoolWillBeFullAsync();
            
            IsPoolReady = true;
            return this;
        }
        
        public TObject Get()
        {
            if (IsPoolReady && Available.Count == 0 && !TryExpandPoolAndPopulate()) return null;

            TObject instance = Available[0];
            Available.RemoveAt(0);
            CurrentlyInUse.Add(instance);
            instance.gameObject.SetActive(true);

            return instance;
        }

        public bool Release(TObject instance)
        {
            if (IsPoolReady && !CurrentlyInUse.Contains(instance)) return false;
            
            CurrentlyInUse.Remove(instance);
            Available.Add(instance);
            instance.gameObject.SetActive(false);

            return true;
        }

        public void ReleaseAll()
        {
            foreach (var instance in CurrentlyInUse)
            {
                Release(instance);
            }
        }

        private bool TryExpandPoolAndPopulate() => TryExpandPool() && TryPopulateUntilPoolWillBeFull();

        private bool TryPopulateUntilPoolWillBeFull()
        {
            if (!IsConfigurationValid()) return false;

            while (TotalCount < CurrentSize)
            {
                TObject newInstance = PoolConfiguration.Fabric.Instantiate(PoolConfiguration.Prefab, Vector3.zero,  Root.transform);
                
                newInstance.gameObject.SetActive(false);
                Available.Add(newInstance);
            }

            return true;
        }
        
        private async UniTask<bool> TryPopulateUntilPoolWillBeFullAsync()
        {
            if (!IsConfigurationValid()) return false;

            while (TotalCount < CurrentSize)
            {
                TObject newInstance = await PoolConfiguration.Fabric.InstantiateAsync(PoolConfiguration.Prefab, Vector3.zero,  Root.transform);
                
                newInstance.gameObject.SetActive(false);
                Available.Add(newInstance);
            }

            return true;
        }

        private bool TryExpandPool()
        {
            if (!PoolConfiguration.Expandable) return false;

            CurrentSize += PoolConfiguration.Size;
            return true;
        }

        public bool IsConfigurationValid()
        {
#if UNITY_EDITOR
            if (PoolConfiguration.Prefab == null)
            {
                Debug.LogError($"[GAME OBJECT POOL] NULL PREFAB WAS FOUND IN {PoolConfiguration.Name}");
                return false;
            }
            if (PoolConfiguration.Fabric == null)
            {
                Debug.LogError($"[GAME OBJECT POOL] NULL FABRIC WAS FOUND IN {PoolConfiguration.Name}");
                return false;
            }

            if (PoolConfiguration.Size == 0)
            {
                Debug.LogError($"[GAME OBJECT POOL] POOL SIZE CANNOT BE 0; {PoolConfiguration.Name}");
                return false;               
            }
#endif

            return PoolConfiguration.Prefab != null && PoolConfiguration.Fabric != null;
        }


        [Serializable]
        public sealed class Config<ConfigTObject>
            where ConfigTObject : Component
        {
            public Config(ConfigTObject prefab, string name = "", ushort size = 32, bool expandable = true, bool persistent = false, IFabric fabric = null)
            {
                if (prefab == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"NULL PREFAB WAS GIVEN TO POOL CONFIG; Pool name: {name}");
#endif
                    return;
                }
                
                Name = name;
                if (Name == "")
                {
                    Name = $"{prefab.name} pool";
                }
                Size = size;
                if (Size == 0) Size = 1;
                Expandable = expandable;
                Persistent = persistent;
                Prefab = prefab;
                Fabric = fabric;

                if (fabric == null)
                {
                    Fabric = new PoolFabric(false, true);
                }
            }

            public string Name; //will be applied to root instance
            
            public ConfigTObject Prefab; //object of pool

            [field: SerializeField] public ushort Size { get; private set; } //size of pool
            
            public bool Expandable; //if true when pool is full it will populate another count of SIZE objects
            
            public bool Persistent; //if true pool will be moved to persistent scene
            
            [HideInInspector] public IFabric Fabric; //custom fabric can be added
        }
    }
}