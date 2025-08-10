using UnityEngine;

namespace DragonBones
{
    [RequireComponent(typeof(UnityEngineArmatureDisplay))]
    public sealed class UnityEngineChildArmatureSlotDisplay : UnityEngineSlotDisplay, IEngineChildArmatureSlotDisplay
    {
        public IEngineArmatureDisplay ArmatureDisplay { get; private set; }

        private void Awake()
        {
            ArmatureDisplay = GetComponent<UnityEngineArmatureDisplay>();
        }
    }
}