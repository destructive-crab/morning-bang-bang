using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    public sealed class UnityArmatureMeshPart : DBObject
    {
        private Armature BelongsTo;
        
        public Material Material { get; set; }
        public Mesh OutputMesh { get; private set; }
        public bool IsSingle => ids.Count == 1;

        //they are not kept with draw order
        private readonly Dictionary<DBRegistry.DBID, CombineInstance> combinesStorage = new();
        private List<DBRegistry.DBID> ids = new();

        //actually used in combining and updating vertices. they are kept with draw order
        private CombineInstance[] combineInstances; //keep in mind that combine instance is a struct
        private DBRegistry.DBID[] currentDrawOrder;

        public UnityArmatureMeshPart(Armature belongsTo)
        {
            BelongsTo = belongsTo;
        }

        public override void OnReleased() { }

        public void Init(DBRegistry.DBID[] slotIDs, Material material)
        {
            Material = material;
            
            currentDrawOrder = slotIDs;
            ids = new List<DBRegistry.DBID>(slotIDs);
        }

        public Mesh Build()
        {
            OutputMesh = UnityDBFactory.GetEmptyMesh();
            combineInstances = new CombineInstance[ids.Count];
            
            for (int i = 0; i < ids.Count; i++)
            {
                CombineInstance combineInstance = new CombineInstance();
                DB.Registry.GetMesh(ids[i]).GenerateMesh();
                combineInstance.mesh = DB.Registry.GetMesh(ids[i]).GeneratedMesh;
                //todo rewrite
                combineInstance.transform = ((UnityArmatureRoot)BelongsTo.Root).transform.localToWorldMatrix * 
                                            ((UnityArmatureRoot)BelongsTo.Root).transform.worldToLocalMatrix;
                
                combinesStorage.Add(ids[i], combineInstance);
                combineInstances[i] = combinesStorage[ids[i]];
            }
            
            OutputMesh.CombineMeshes(combineInstances);
            
            return OutputMesh;
        }

        public void UpdateVertices()
        {
            int offset = 0;
            Vector3[] vertexBuffer = OutputMesh.vertices;

            foreach (DBRegistry.DBID id in ids)
            {
                DBMeshBuffer buffer = DB.Registry.GetMesh(id);
                for (var i = 0; i < buffer.vertexBuffer.Length; i++)
                {
                    vertexBuffer[i+offset] = buffer.vertexBuffer[i];
                }

                offset += buffer.VertexCount;
            }

            OutputMesh.vertices = vertexBuffer;
        }

        public Mesh RebuildWithDifferentOrder(DBRegistry.DBID[] slotOrder)
        {
            if (slotOrder.Length != combineInstances.Length)
            {
                DBLogger.Warn("NEW SLOTS ORDER COUNT DOES NOT MATCH CURRENT SLOTS COUNT");
                return OutputMesh;
            }

            if (OutputMesh == null)
            {
                DBLogger.Warn("REBUILD FAILED. NEED TO BUILD A MESH FIRST");
                return null;
            }
            
            for (int i = 0; i < slotOrder.Length; i++)
            {
                DBRegistry.DBID slot = slotOrder[i];

                combineInstances[i] = new CombineInstance();
                
                DB.Registry.GetMesh(slot).GenerateMesh();
                
                combineInstances[i].mesh = DB.Registry.GetMesh(slot).GeneratedMesh;
                
                combineInstances[i].transform = ((UnityArmatureRoot)BelongsTo.Root).transform.localToWorldMatrix * 
                                            ((UnityArmatureRoot)BelongsTo.Root).transform.worldToLocalMatrix;

            }
            
            OutputMesh.Clear();
            OutputMesh.CombineMeshes(combineInstances);
            
            return OutputMesh;
        }

        public bool Contains(DBRegistry.DBID id) => ids.Contains(id);
        public void SendChange(ArmatureRegistry.RegistryChange change, DBRegistry.DBID slotName)
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