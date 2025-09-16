using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    public sealed class UnityArmatureMeshPart : DBObject
    {
        public Mesh OutputMesh { get; private set; }
        
        public bool IsSingle => allBuffers.Count == 1;
        
        public bool IsDirty = false;

        public int From { get; private set; } = -1;
        public int To { get; private set; } = -1;

        public Material Material { get; private set; }
        //they are not kept with draw order
        private readonly List<DBMeshBuffer> allBuffers = new();
        private readonly Dictionary<DBMeshBuffer, CombineInstance> combinesStorage = new();

        //actually used in combining and updating vertices. they are kept with draw order
        private CombineInstance[] combineInstances; //keep in mind that combine instance is a struct
        private List<DBMeshBuffer> currentDrawOrder;
        private UnityArmatureMeshRoot Parent;
        private Armature Armature;

        private bool ready = false;
        private bool isBuilt = false;

        public void Init(DBMeshBuffer[] buffers, Material material, UnityArmatureMeshRoot meshRoot)
        {
            Material = material;
            allBuffers.AddRange(buffers);
            
            currentDrawOrder = new List<DBMeshBuffer>(buffers);
            currentDrawOrder.Capacity = this.allBuffers.Count;

            Parent = meshRoot;
            Armature = meshRoot.BelongsTo.Armature;
            ready = true;
        }

        public void SetRange(int from, int to)
        {
            From = from;
            To = to;
        }
        
        public override void OnReleased()
        {
            allBuffers.Clear();
            combinesStorage.Clear();

            combineInstances = null;
            currentDrawOrder.Clear();
            Parent = null;

            Material = null;
            OutputMesh.Clear();
            From = -1;
            To = -1;
            IsDirty = false;
            isBuilt = false;
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
                combineInstance.transform = Parent.BelongsTo.transform.localToWorldMatrix * 
                                            Parent.BelongsTo.transform.worldToLocalMatrix;
                
                combineInstances[i] = combineInstance;
            }
            
            OutputMesh.CombineMeshes(combineInstances);
            OutputMesh.RecalculateBounds();
            IsDirty = true;
            isBuilt = true;
            
            return OutputMesh;
        }
        
        public Mesh RebuildWithDifferentOrder()
        {
            if (OutputMesh == null || !isBuilt || !ready)
            {
                DBLogger.Warn("REBUILD FAILED. NEED TO INIT AND BUILD A MESH FIRST");
                return null;
            }

            int currentLocal = 0;
            for (int currentGlobal = From; currentGlobal < To && currentLocal < currentDrawOrder.Count; currentGlobal++)
            {
                DBMeshBuffer buffer = Parent.GetAtDrawOrder(currentGlobal);
                
                if(!buffer.AttachedTo.Visible.V)
                {
                    continue;
                }

                if (currentLocal < 0 || currentLocal >= currentDrawOrder.Count)
                {
                    DBLogger.LogMessage(currentLocal + " " + currentDrawOrder.Count);
                }
                currentDrawOrder[currentLocal] = buffer;

                combineInstances[currentLocal].mesh = buffer.GeneratedMesh;
                currentLocal++;
            }
            
            OutputMesh.Clear();
            OutputMesh.CombineMeshes(combineInstances);
            
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
        private void UpdateVisibilities()
        {
            bool added = false;
            
            for (var i = 0; i < allBuffers.Count; i++)
            {
                if (!allBuffers[i].AttachedTo.Visible.V)
                {
                    currentDrawOrder.Remove(allBuffers[i]);
                }
                else if (!currentDrawOrder.Contains(allBuffers[i]))
                {
                    currentDrawOrder.Add(allBuffers[i]);
                    added = true;
                }
            }

            if (added)
            {
                currentDrawOrder.Sort(CompareBuffers);
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
        
        public bool Contains(DBMeshBuffer buffer) => allBuffers.Contains(buffer);
    }
}