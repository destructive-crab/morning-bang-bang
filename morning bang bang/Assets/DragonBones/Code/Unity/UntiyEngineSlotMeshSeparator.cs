using UnityEngine;
using System.Collections.Generic;

namespace DragonBones
{
    public sealed class UnityEngineMeshSeparator
    {
        public UnityEngineArmatureDisplay BelongsTo;

        public bool IsCreated { get; private set;}
        private Dictionary<string, UnityEngineSeparatedSlotDisplay> connectedDisplays = new();

        public UnityEngineMeshSeparator(UnityEngineArmatureDisplay belongsTo)
        {
            BelongsTo = belongsTo;
        }

        #region API
        public void Create()
        {
            foreach(var slot in BelongsTo.Armature.Structure.Slots)
            {
                connectedDisplays.Add(slot.Name, CreateNewDisplay());
                connectedDisplays[slot.Name].Connect(slot as UnitySlot);
            }
        }
        public void Update()
        {
            //todo check if dirty


            //update vertices
            foreach(var connectedDisplayPair in connectedDisplays)
            {
                UpdateVerticesOf(connectedDisplayPair.Value);
            }

        }
        #endregion

        private void UpdateVerticesOf(UnityEngineSeparatedSlotDisplay display)
        {
            display.MeshFilter.sharedMesh.vertices = display.ConnectedWith.MeshBuffer.vertexBuffer;
        }

        private UnityEngineSeparatedSlotDisplay CreateNewDisplay()
        {
            return new GameObject().AddComponent<UnityEngineSeparatedSlotDisplay>();
        }

        public bool Contains(string slotName)
        {
            return connectedDisplays.ContainsKey(slotName);
        }
    }
}
