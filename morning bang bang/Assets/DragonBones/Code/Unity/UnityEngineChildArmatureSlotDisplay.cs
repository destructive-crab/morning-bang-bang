using UnityEngine;

namespace DragonBones
{
    [RequireComponent(typeof(UnityEngineArmatureDisplay))]
    public sealed class UnityEngineChildArmatureSlotDisplay : MonoBehaviour, IEngineChildArmatureSlotDisplay
    {
        public ChildArmatureDisplayData Data { get; private set; }
        public IEngineArmatureDisplay ArmatureDisplay { get; private set; }
        
        public void Enable()
        {
            
        }

        public void Disable()
        {
        }

        private void Awake()
        {
            ArmatureDisplay = GetComponent<UnityEngineArmatureDisplay>();
        }

        public void Build(ChildArmatureDisplayData childArmatureData, UnitySlot forSlot)
        {
            Data = childArmatureData;
            transform.parent = forSlot.ArmatureDisplay.transform;
        }
    }
}