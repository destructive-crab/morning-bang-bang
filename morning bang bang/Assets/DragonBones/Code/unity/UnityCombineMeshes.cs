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
        private Mesh combinedMesh;
        private List<CombineInstance> cs1;

        public UnityEngineMeshCombiner(UnityEngineArmatureDisplay belongsTo)
        {
            BelongsTo = belongsTo;
        }

        public void Clear()
        {
            
        }
        
        public void Combine()
        {
            cs1 = new();
            foreach (Slot slot in BelongsTo.Armature.Structure.Slots)
            {
                UnitySlot unitySlot = (UnitySlot)slot;
                
                if(unitySlot.IsDisplayingChildArmature()) continue;
                
                Combines.Add(new CombineSlot(unitySlot, new CombineInstance()));

                CombineSlot currentCombine = Combines.Last();

                currentCombine.CombineInstance.mesh = unitySlot.meshBuffer.sharedMesh;
                currentCombine.CombineInstance.transform = unitySlot.CurrentAsMeshDisplay.MeshFilter.transform.localToWorldMatrix;
                
                cs1.Add(currentCombine.CombineInstance);
                unitySlot.Disable();
            }

            combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(cs1.ToArray());
            
            BelongsTo.gameObject.AddComponent<MeshFilter>().sharedMesh = combinedMesh;
            BelongsTo.gameObject.AddComponent<MeshRenderer>().material = Combines[0].Slot.currentTextureAtlasData.texture;

            IsCombined = true;
        }

        public void Update()
        {
             combinedMesh.CombineMeshes(cs1.ToArray());
             BelongsTo.gameObject.GetComponent<MeshFilter>().sharedMesh = combinedMesh;
        }
    }
}