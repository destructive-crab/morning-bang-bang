using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DragonBones
{
    public static class DB
    {
        public static DBRegistry Registry => Kernel.Registry;
        
        private static GameObject _gameObject;
        public static DBKernel Kernel { get; private set; }
        
        public static UnityDBFactory Factory { get; private set; }
        public static UnityDataLoader UnityDataLoader { get; private set; }
        
        public static async UniTask InitializeDragonBones()
        {
            UnityDataLoader = new UnityDataLoader();
            Factory = new UnityDBFactory();

            await Factory.InitializeFactory();
            
            if (Application.isPlaying)
            {
                if (_gameObject == null)
                {
                    _gameObject = GameObject.Find("DragonBones Object");
                    
                    if (_gameObject == null)
                    {
                        _gameObject = new GameObject("DragonBones Object");

                        _gameObject.isStatic = true;
                    }
                }

                GameObject.DontDestroyOnLoad(_gameObject);

                var eventManager = _gameObject.GetComponent<UnityEventDispatcher>();
                if (eventManager == null)
                {
                    eventManager = _gameObject.AddComponent<UnityEventDispatcher>();
                }

                if (Kernel == null)
                {
                    Kernel = new DBKernel(eventManager, Factory);
                    //
                    DBKernel.IsNegativeYDown = false;
                }
            }
            else
            {
                if (Kernel == null)
                {
                    Kernel = new DBKernel(null, Factory);
                    //
                    DBKernel.IsNegativeYDown = false;
                }
            }
            
            DBLogger.LogMessage("DragonBones was successfully initialized");
        }
    }
}