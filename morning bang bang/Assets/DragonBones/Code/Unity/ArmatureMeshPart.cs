using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    public sealed class ArmatureMeshPart : DBObject
    {
        public Mesh sharedMesh;

        private UnityEngineArmatureDisplay BelongsTo;

        public Material Material { get; private set; }
        
        //they are not kept with draw order
        private readonly Dictionary<string, MeshBuffer> buffersStorage = new(); //key - slot name
        private readonly Dictionary<string, CombineInstance> combinesStorage = new();
        
        //actually used in combining and updating vertices. they are kept with draw order
        private CombineInstance[] combineInstances; //keep in mind that combine instance is a struct
        private MeshBuffer[] buffers;

        public ArmatureMeshPart(UnityEngineArmatureDisplay belongsTo)
        {
            BelongsTo = belongsTo;
        }

        //clearing
        public override void OnReleased()
        {
            buffersStorage.Clear();
            
        }

        public void UpdateVertices()
        {
            int offset = 0;
            Vector3[] vertexBuffer = sharedMesh.vertices;

            foreach (MeshBuffer buffer in buffers)
            {
                for (var i = 0; i < buffer.vertexBuffer.Length; i++)
                {
                    vertexBuffer[i+offset] = buffer.vertexBuffer[i];
                }

                offset += buffer.vertexBuffer.Length;
            }

            sharedMesh.vertices = vertexBuffer;
        }
        
        public Mesh Build(UnitySlot[] slots, Material material)
        {
            if (slots.Length == 1)
            {
                sharedMesh = slots[0].MeshBuffer.InitMesh();
                buffersStorage.Add(slots[0].Name, slots[0].MeshBuffer);
                buffers = new MeshBuffer[1];
                buffers[0] = slots[0].MeshBuffer;
            }
            else
            {
                combineInstances = new CombineInstance[slots.Length];
                buffers = new MeshBuffer[slots.Length];
                
                for (var index = 0; index < slots.Length; index++)
                {
                    MeshBuffer meshBuffer = slots[index].MeshBuffer;

                    //?todo
                    combineInstances[index] = new CombineInstance();
                    buffers[index] = slots[index].MeshBuffer;
                    
                    combineInstances[index].mesh = meshBuffer.InitMesh();
                    combineInstances[index].transform = BelongsTo.transform.localToWorldMatrix * BelongsTo.transform.worldToLocalMatrix;
                    
                    buffersStorage.Add(slots[index].Name, meshBuffer);
                    combinesStorage.Add(slots[index].Name, combineInstances[index]);
                }

                sharedMesh = new Mesh();
                sharedMesh.CombineMeshes(combineInstances);
            }

            Material = material;
            
            return sharedMesh;
        }
        public Mesh RebuildWithDifferentOrder(string[] slotOrder)
        {
            if (slotOrder.Length != combineInstances.Length)
            {
                DBLogger.LogWarning("NEW SLOTS ORDER COUNT DOES NOT MATCH CURRENT SLOTS COUNT");
                return sharedMesh;
            }

            if (sharedMesh == null)
            {
                DBLogger.LogWarning("REBUILD FAILED. NEED TO BUILD A MESH FIRST");
                return null;
            }
            
            for (int i = 0; i < slotOrder.Length; i++)
            {
                string slot = slotOrder[i];

                combineInstances[i] = combinesStorage[slot];
                buffers[i] = buffersStorage[slot];
            }
            
            sharedMesh.CombineMeshes(combineInstances);
            return sharedMesh;
        }

        public bool Contains(string slotName)
        {
            return buffersStorage.ContainsKey(slotName);
        }
    }
}