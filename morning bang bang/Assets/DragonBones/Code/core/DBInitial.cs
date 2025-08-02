using UnityEngine;

namespace DragonBones
{
    public static class DBInitial
    {
        private static GameObject _gameObject;
        public static DBKernel Kernel { get; private set; }
        
        public static DBUnityFactory UnityFactory { get; private set; }
        public static UnityDataLoader UnityDataLoader { get; private set; }
        
        public static bool InitializeDragonBones()
        {
            UnityDataLoader = new();
            UnityFactory = new();
            
            Debug.Log("DragonBones Factory Initialized");
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

                var eventManager = _gameObject.GetComponent<DragonBoneEventDispatcher>();
                if (eventManager == null)
                {
                    eventManager = _gameObject.AddComponent<DragonBoneEventDispatcher>();
                }

                if (Kernel == null)
                {
                    Kernel = new DBKernel(eventManager, UnityFactory);
                    //
                    DBKernel.yDown = false;
                }
            }
            else
            {
                if (Kernel == null)
                {
                    Kernel = new DBKernel(null, UnityFactory);
                    //
                    DBKernel.yDown = false;
                }
            }
            
            return false;
        }
    }
}