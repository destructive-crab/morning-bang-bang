using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DragonBones
{
    public sealed class UnityArmatureMeshRoot 
    {
        public bool IsCombined { get; private set; } = false;

        public readonly UnityArmatureRoot BelongsTo;
        public Armature Armature => BelongsTo.Armature;

        private readonly List<UnityArmatureMeshPart> parts = new();

        private bool meshesChanged = false;
        private readonly List<DBMeshBuffer> meshes = new();
        private readonly Dictionary<string, DBMeshBuffer> meshesMap = new();
        
        
        public Mesh OutputMesh;
        private Material[] materials;
        
        private readonly MeshFilter filter;
        private readonly MeshRenderer renderer;
        
        public UnityArmatureMeshRoot(UnityArmatureRoot belongsTo, MeshFilter meshFilter, MeshRenderer meshRenderer)
        {
            BelongsTo = belongsTo;
            filter = meshFilter;
            renderer = meshRenderer;
            Debug.Log(filter);
        }

        public DBMeshBuffer GetMeshFor(Slot slot)
        {
            DBMeshBuffer mesh = DBMeshBuffer.BorrowObject<DBMeshBuffer>();
            
            meshes.Add(mesh);
            meshesMap.Add(GetSlotID(slot), mesh);

            return mesh;
        }
       
        public void Update()
        {
            List<Vector3> vertices = new();
            List<Vector2> uvs = new();
            
            foreach (UnityArmatureMeshPart meshPart in parts)
            {
                meshPart.UpdateVertices();
                
                vertices.AddRange(meshPart.OutputMesh.vertices);
                uvs.AddRange(meshPart.OutputMesh.uv);
            }
            
            OutputMesh.vertices = vertices.ToArray();
            OutputMesh.uv = uvs.ToArray();
            
            OutputMesh.RecalculateBounds();
        }

        public void Combine()
        {
            CollectParts();
            
            List<Vector3> vertices = new();
            List<Vector2> uvs = new();
            List<int[]> triangles = new();
            
            foreach (UnityArmatureMeshPart meshPart in parts)
            {
                triangles.Add(meshPart.OutputMesh.triangles);
              
                for (var i = 0; i < triangles.Last().Length; i++)
                {
                    triangles.Last()[i] += vertices.Count;
                }
                
                vertices.AddRange(meshPart.OutputMesh.vertices);
                uvs.AddRange(meshPart.OutputMesh.uv);
            }

            OutputMesh = UnityDBFactory.GetEmptyMesh();

            OutputMesh.vertices = vertices.ToArray();
            OutputMesh.uv = uvs.ToArray();

            OutputMesh.subMeshCount= triangles.Count;

            materials = new Material[parts.Count];
            for (var i = 0; i < triangles.Count; i++)
            {
                int[] triangle = triangles[i];
                OutputMesh.SetTriangles(triangle, i);
                materials[i] = (parts[i].Material);
            }

            OutputMesh.RecalculateBounds();

            filter.sharedMesh = OutputMesh;
            renderer.sharedMaterials = materials;
            
            IsCombined = true;
        }

        private void CollectParts()
        {
            List<DBMeshBuffer> currentPart = new();
            Material currentMaterial = null;

            for (int i = 0; i < meshes.Count; i++)
            {
                DBMeshBuffer meshBuffer = meshes[i];
                
                if(meshBuffer == null) continue;
                if (meshBuffer.Material == currentMaterial || currentMaterial == null)
                {
                    currentPart.Add(meshBuffer);
                    currentMaterial = meshBuffer.Material;
                }
                else
                {
                    parts.Add(new UnityArmatureMeshPart(Armature));
                    parts.Last().Init(currentPart.ToArray(), currentMaterial);
                    currentPart.Clear();
                    currentMaterial = meshBuffer.Material;
                    i--;
                }
            }

            if (currentPart.Count != 0)
            {
                parts.Add(new UnityArmatureMeshPart(Armature));
                parts.Last().Init(currentPart.ToArray(), currentMaterial);
                currentPart.Clear();
            }
            
            foreach (UnityArmatureMeshPart part in parts)
            {
                part.Build();
            }
        }

        public UnityArmatureMeshPart GetPartWith(DBMeshBuffer buffer)
        {
            return parts.Find((part) => part.Contains(buffer));
        }

        public void SendChange(ArmatureRegistry.RegistryChange change, Slot slot)
        {
            string slotID = GetSlotID(slot);
            
            if (!meshesMap.ContainsKey(slotID)) return;
            
            UnityArmatureMeshPart part = GetPartWith(meshesMap[slotID]);
            
            if (change == ArmatureRegistry.RegistryChange.DrawOrder && part.IsSingle && parts.Count != 1)
            {
                //recombine
                Combine();
            }
            else
            {
                part.SendChange(change, meshesMap[slotID]);
            }
            
            return;
            switch (change)
            {
                case ArmatureRegistry.RegistryChange.Visibility:
                    
                    break;
                case ArmatureRegistry.RegistryChange.Display:
                    break;
                case ArmatureRegistry.RegistryChange.DrawOrder:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(change), change, null);
            }
        }

        private static string GetSlotID(Slot slot)
        {
            return slot.Name + "_" + slot.GetHashCode();
        }

        public void ReleaseMesh(UnitySlot unitySlot)
        {
            string slotID = GetSlotID(unitySlot);
            if(!meshesMap.ContainsKey(slotID))return;
            meshes.Remove(meshesMap[slotID]);
            meshesMap.Remove(slotID);
        }

        public void Clear()
        {
            
        }
    }
}