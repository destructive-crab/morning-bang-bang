using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DragonBones
{
    public static class DBInitial
    {
        private static GameObject _gameObject;
        public static DBKernel Kernel { get; private set; }
        
        public static DBUnityFactory UnityFactory { get; private set; }
        public static UnityDataLoader UnityDataLoader { get; private set; }
        
        public static async UniTask InitializeDragonBones()
        {
            UnityDataLoader = new UnityDataLoader();
            UnityFactory = new DBUnityFactory();

            await UnityFactory.InitializeFactory();
            
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
                    Kernel = new DBKernel(eventManager, UnityFactory);
                    //
                    DBKernel.IsNegativeYDown = false;
                }
            }
            else
            {
                if (Kernel == null)
                {
                    Kernel = new DBKernel(null, UnityFactory);
                    //
                    DBKernel.IsNegativeYDown = false;
                }
            }
            
            DBLogger.LogMessage("DragonBones was successfully initialized");
        }
    }
}