using UnityEngine;

namespace DragonBones
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class UnityEngineSlotDisplay : MonoBehaviour, IEngineSlotDisplay
    {
        public Slot Parent { get; }
        public DisplayData Data { get; private set; }
        
        public MeshRenderer MeshRenderer { get; private set; }
        public MeshFilter MeshFilter { get; private set; }

        private void Awake()
        {
            MeshRenderer = GetComponent<MeshRenderer>();
            MeshFilter = GetComponent<MeshFilter>();
        }
        
        public void Init(DisplayData data)
        {
            Data = data;
            gameObject.name = data.Name;
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public void SetEnabled(bool parentVisible)
        {
            switch (parentVisible)
            {
                case true:
                    Enable();
                    break;
                case false:
                    Disable();
                    break;
            }
        }
        
        public void Build(UnitySlot slot)
        {
            DBLogger.LogMessage(transform + " " + slot);
            
            transform.name = slot.Name;
            transform.parent = slot.ArmatureDisplay.transform;
            
            foreach (var display in slot.Displays.GetChildArmaturesDisplays)
            {
                (display.ArmatureDisplay as UnityEngineArmatureDisplay).transform.parent = transform;
            }

            (slot.Displays.MeshDisplay as UnityEngineSlotDisplay).transform.parent = transform;
        }
    }
}