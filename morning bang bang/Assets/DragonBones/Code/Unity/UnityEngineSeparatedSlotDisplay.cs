using UnityEngine;

namespace DragonBones
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public sealed class UnityEngineSeparatedSlotDisplay : MonoBehaviour
    {
        public MeshRenderer MeshRenderer { get; private set; }
        public MeshFilter MeshFilter { get; private set; }

        public UnitySlot ConnectedWith { get; private set;}

        private void Awake()
        {
            MeshRenderer = GetComponent<MeshRenderer>();
            MeshFilter = GetComponent<MeshFilter>();
        }

        public void Connect(UnitySlot slot)
        {
            ConnectedWith = slot;
        }
    }
}
