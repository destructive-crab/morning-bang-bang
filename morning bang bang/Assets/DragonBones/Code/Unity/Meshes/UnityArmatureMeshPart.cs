using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    public sealed class UnityArmatureMeshPart : DBObject
    {
        private Armature BelongsTo;
        
        public Material Material { get; set; }
        public Mesh OutputMesh { get; private set; }
        public bool IsSingle => buffers.Count == 1;

        //they are not kept with draw order
        private readonly Dictionary<DBMeshBuffer, CombineInstance> combinesStorage = new();
        private List<DBMeshBuffer> buffers = new();

        //actually used in combining and updating vertices. they are kept with draw order
        private CombineInstance[] combineInstances; //keep in mind that combine instance is a struct
        private DBMeshBuffer[] currentDrawOrder;

        public UnityArmatureMeshPart(Armature belongsTo)
        {
            BelongsTo = belongsTo;
        }

        public override void OnReleased() { }

        public void Init(DBMeshBuffer[] buffers, Material material)
        {
            Material = material;
            this.buffers = new List<DBMeshBuffer>(buffers);
            currentDrawOrder = buffers;
        }

        public Mesh Build()
        {
            OutputMesh = UnityDBFactory.GetEmptyMesh();
            combineInstances = new CombineInstance[buffers.Count];
            
            for (int i = 0; i < buffers.Count; i++)
            {
                CombineInstance combineInstance = new CombineInstance();
                buffers[i].GenerateMesh();
                combineInstance.mesh = buffers[i].GeneratedMesh;
                //todo rewrite
                combineInstance.transform = ((UnityArmatureRoot)BelongsTo.Root).transform.localToWorldMatrix * 
                                            ((UnityArmatureRoot)BelongsTo.Root).transform.worldToLocalMatrix;
                
                combinesStorage.Add(buffers[i], combineInstance);
                combineInstances[i] = combinesStorage[buffers[i]];
            }
            
            OutputMesh.CombineMeshes(combineInstances);
            
            return OutputMesh;
        }

        public void UpdateVertices()
        {
            int offset = 0;
            Vector3[] vertexBuffer = OutputMesh.vertices;

            foreach (DBMeshBuffer buffer in buffers)
            {
                for (var i = 0; i < buffer.vertexBuffer.Length; i++)
                {
                    vertexBuffer[i+offset] = buffer.vertexBuffer[i];
                }

                offset += buffer.VertexCount;
            }

            OutputMesh.vertices = vertexBuffer;
        }

        public Mesh RebuildWithDifferentOrder(DBMeshBuffer[] newOrder)
        {
            if (newOrder.Length != combineInstances.Length)
            {
                DBLogger.Warn("NEW SLOTS ORDER COUNT DOES NOT MATCH CURRENT SLOTS COUNT");
                return OutputMesh;
            }

            if (OutputMesh == null)
            {
                DBLogger.Warn("REBUILD FAILED. NEED TO BUILD A MESH FIRST");
                return null;
            }

            currentDrawOrder = newOrder;
            
            for (int i = 0; i < newOrder.Length; i++)
            {
                DBMeshBuffer buffer = newOrder[i];

                combineInstances[i] = new CombineInstance();
                
                combineInstances[i].mesh = buffer.GeneratedMesh;
                combineInstances[i].transform = ((UnityArmatureRoot)BelongsTo.Root).transform.localToWorldMatrix * 
                                            ((UnityArmatureRoot)BelongsTo.Root).transform.worldToLocalMatrix;
            }
            
            OutputMesh.Clear();
            OutputMesh.CombineMeshes(combineInstances);
            
            return OutputMesh;
        }

        public bool Contains(DBMeshBuffer buffer) => buffers.Contains(buffer);
        public void SendChange(ArmatureRegistry.RegistryChange change, DBMeshBuffer slotName)
        {
            switch (change)
            {
                case ArmatureRegistry.RegistryChange.Visibility:
                    break;
                case ArmatureRegistry.RegistryChange.Display:
                    break;
                case ArmatureRegistry.RegistryChange.DrawOrder:
                    Sort();
                    RebuildWithDifferentOrder(currentDrawOrder);
                    break;
            }
        }

        private void Sort()
        {
//            for (int i = PartBeginning; i < PartBeginning+PartLength; i++)
//            {
//                currentDrawOrder[i - PartBeginning] = BelongsTo.Structure.GetAtDrawOrder(i);
//            }
        }
    }
}