using MothDIed.Pool;
using UnityEngine;

namespace DragonBones
{
    public class UnityEngineSlotDisplay : MonoBehaviour, IEngineSlotDisplay, IPoolableGameObject<UnityEngineSlotDisplay>
    {
        public Slot Parent { get; protected set; }
        public DisplayData Data { get; private set; }

        public void Enable() => gameObject.SetActive(true);
        public void Disable() => gameObject.SetActive(false);
        public void SetEnabled(bool parentVisible)
        {
            switch (parentVisible)
            {
                case true: Enable(); break;
                case false: Disable(); break;
            }
        }
        
        public void Build(DisplayData data, UnitySlot parent)
        {
            DBLogger.BLog.AddEntry("Build", "Unity Mesh Slot Display");
            transform.name = data.Name;
            transform.parent = parent.ArmatureDisplay.transform;

            Data = data;
            Parent = parent;
        }

        public GameObjectPool<UnityEngineSlotDisplay> Pool { get; private set; }

        public void OnPopulated(GameObjectPool<UnityEngineSlotDisplay> pool) => Pool = pool;
        public void ReleaseThis() => Pool.Release(this);

        public virtual void OnPicked() { }
        public virtual void OnReleased() 
        { 
            Data = null;
            Parent = null;
        }
    }
}