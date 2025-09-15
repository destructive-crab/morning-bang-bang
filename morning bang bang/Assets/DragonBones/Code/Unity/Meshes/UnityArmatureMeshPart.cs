using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    public sealed class UnityArmatureMeshPart : DBObject
    {
        public bool IsDirty = false;
        
        public int From;
        public int To;
        
        private Armature BelongsTo;
        
        public Material Material { get; set; }
        public Mesh OutputMesh { get; private set; }
        public bool IsSingle => buffers.Count == 1;

        //they are not kept with draw order
        private readonly Dictionary<DBMeshBuffer, CombineInstance> combinesStorage = new();
        private List<DBMeshBuffer> buffers = new();

        //actually used in combining and updating vertices. they are kept with draw order
        private CombineInstance[] combineInstances; //keep in mind that combine instance is a struct
        private List<DBMeshBuffer> currentDrawOrder;
        private UnityArmatureMeshRoot meshRoot;

        public UnityArmatureMeshPart(Armature belongsTo)
        {
            BelongsTo = belongsTo;
        }

        public override void OnReleased() { }

        public void Init(DBMeshBuffer[] buffers, Material material, UnityArmatureMeshRoot meshRoot)
        {
            Material = material;
            this.buffers = new List<DBMeshBuffer>(buffers);
            
            currentDrawOrder = new List<DBMeshBuffer>(buffers);
            currentDrawOrder.Capacity = this.buffers.Count;
            
            this.meshRoot = meshRoot;
        }

        public Mesh Build()
        {
            if (OutputMesh != null)
            {
                OutputMesh.Clear();
            }
            else
            {
                OutputMesh = UnityDBFactory.GetEmptyMesh();
            }

            combineInstances = new CombineInstance[currentDrawOrder.Count];               
            
            for (int i = 0; i < currentDrawOrder.Count; i++)
            {
                combinesStorage.TryAdd(currentDrawOrder[i], new CombineInstance());

                CombineInstance combineInstance = combinesStorage[currentDrawOrder[i]];
                
                currentDrawOrder[i].GenerateMesh();
                
                combineInstance.mesh = currentDrawOrder[i].GeneratedMesh;
                combineInstance.transform = meshRoot.BelongsTo.transform.localToWorldMatrix * 
                                            meshRoot.BelongsTo.transform.worldToLocalMatrix;
                
                combineInstances[i] = combineInstance;
            }
            
            OutputMesh.CombineMeshes(combineInstances);
            OutputMesh.RecalculateBounds();
            IsDirty = true;
            
            return OutputMesh;
        }

        public void UpdateVertices()
        {
            int offset = 0;
            Vector3[] vertexBuffer = OutputMesh.vertices;

            foreach (DBMeshBuffer buffer in currentDrawOrder)
            {
                for (var i = 0; i < buffer.vertexBuffer.Length; i++)
                {
                    vertexBuffer[i+offset] = buffer.vertexBuffer[i];
                }

                offset += buffer.VertexCount;
            }

            OutputMesh.vertices = vertexBuffer;
        }

        public Mesh RebuildWithDifferentOrder()
        {
            if (OutputMesh == null)
            {
                DBLogger.Warn("REBUILD FAILED. NEED TO BUILD A MESH FIRST");
                return null;
            }

            int currentLocal = 0;
            for (int currentGlobal = From; currentGlobal < To; currentGlobal++)
            {
                DBMeshBuffer buffer = meshRoot.GetAtDrawOrder(currentGlobal);
                
                if(!buffer.AttachedTo.Visible.V)
                {
                    continue;
                }

                currentDrawOrder[currentLocal] = buffer;

                combineInstances[currentLocal].mesh = buffer.GeneratedMesh;
                currentLocal++;
            }
            
            OutputMesh.Clear();
            OutputMesh.CombineMeshes(combineInstances);
            
            return OutputMesh;
        }

        public bool Contains(DBMeshBuffer buffer) => buffers.Contains(buffer);
        public void SendChange(ArmatureRegistry.RegistryChange change, DBMeshBuffer buffer)
        {
            switch (change)
            {
                case ArmatureRegistry.RegistryChange.Visibility:
                    UpdateVisibilities();
                    Build();
                    break;
                case ArmatureRegistry.RegistryChange.Display:
                    Build();
                    break;
                case ArmatureRegistry.RegistryChange.DrawOrder:
                    RebuildWithDifferentOrder();
                    break;
            }
        }

        private void UpdateDrawOrder()
        {
            currentDrawOrder.Sort(CompareBuffers);

            for (var i = 0; i < currentDrawOrder.Count; i++)
            {
                DBMeshBuffer buffer = currentDrawOrder[i];
                combineInstances[i] = combinesStorage[buffer];
            }
        }

        private static int CompareBuffers(DBMeshBuffer x, DBMeshBuffer y)
        {
            if (x.AttachedTo.DrawOrder.V < y.AttachedTo.DrawOrder.V)
            {
                return 1;
            }

            if (x.AttachedTo.DrawOrder.V > y.AttachedTo.DrawOrder.V)
            {
                return -1;
            }

            return 0;
        }

        private void UpdateVisibilities()
        {
            bool added = false;
            
            for (var i = 0; i < buffers.Count; i++)
            {
                if (!buffers[i].AttachedTo.Visible.V)
                {
                    currentDrawOrder.Remove(buffers[i]);
                }
                else if (!currentDrawOrder.Contains(buffers[i]))
                {
                    currentDrawOrder.Add(buffers[i]);
                    added = true;
                }
            }

            if (added)
            {
                currentDrawOrder.Sort(CompareBuffers);
            }
        }
    }
}