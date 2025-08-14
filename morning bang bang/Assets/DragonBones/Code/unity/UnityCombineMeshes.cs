using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DragonBones
{
    public sealed class UnityEngineMeshCombiner
    {
        public sealed class CombineSlot
        {
            public UnitySlot Slot;
            public CombineInstance CombineInstance;

            public CombineSlot(UnitySlot slot, CombineInstance combineInstance)
            {
                Slot = slot;
                CombineInstance = combineInstance;
            }
        }

        public bool IsCombined { get; private set; } = false;
        public readonly UnityEngineArmatureDisplay BelongsTo;

        private List<CombineSlot> Combines = new();
        
        public UnityEngineMeshCombiner(UnityEngineArmatureDisplay belongsTo)
        {
            BelongsTo = belongsTo;
        }

        public void Clear()
        {
            
        }
        
        public void Combine()
        {
            foreach (Slot slot in BelongsTo.Armature.Structure.Slots)
            {
                UnitySlot unitySlot = (UnitySlot)slot;
                
                if(unitySlot.IsDisplayingChildArmature()) continue;
                
                Combines.Add(new CombineSlot(unitySlot, new CombineInstance()));

                CombineSlot currentCombine = Combines.Last();

                currentCombine.CombineInstance.mesh = unitySlot.meshBuffer.sharedMesh;
                currentCombine.CombineInstance.transform = unitySlot.CurrentAsMeshDisplay.MeshFilter.transform.localToWorldMatrix;
            }
        }
    }
}