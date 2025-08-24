using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace DragonBones
{
    public sealed class UnityArmatureMeshRoot 
    {
        public bool IsCombined { get; private set; } = false;

        public readonly Armature BelongsTo;

        private readonly List<UnityArmatureMeshPart> parts = new();
        private readonly List<UnityChildArmature> childArmatures = new();

        public Mesh OutputMesh;
        private Material[] materials;
        
        private MeshFilter filter;
        private MeshRenderer renderer;
        
        public UnityArmatureMeshRoot(Armature belongsTo, MeshFilter meshFilter, MeshRenderer meshRenderer)
        {
            BelongsTo = belongsTo;
            filter = meshFilter;
            renderer = meshRenderer;
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
            List<DBRegistry.DBID> currentPart = new();
            Material currentMaterial = null;

            DBRegistry.DBID[] slots = DB.Registry.GetChildSlotsOf(BelongsTo.ID, true);
            for (int i = 0; i < slots.Length; i++)
            {
                DBRegistry.DBID slotID = slots[i];

                DBMeshBuffer meshBuffer = DB.Registry.GetMesh(slotID);
                if(meshBuffer == null) continue;
                if (meshBuffer.Material == currentMaterial || currentMaterial == null)
                {
                    currentPart.Add(slotID);
                    currentMaterial = meshBuffer.Material;
                }
                else
                {
                    parts.Add(new UnityArmatureMeshPart(BelongsTo));
                    parts.Last().Init(currentPart.ToArray(), currentMaterial);
                    currentPart.Clear();
                    currentMaterial = meshBuffer.Material;
                    i--;
                }
            }

            if (currentPart.Count != 0)
            {
                parts.Add(new UnityArmatureMeshPart(BelongsTo));
                parts.Last().Init(currentPart.ToArray(), currentMaterial);
                currentPart.Clear();
            }
            
            foreach (UnityArmatureMeshPart part in parts)
            {
                part.Build();
            }
        }

        public UnityArmatureMeshPart GetPartWith(DBRegistry.DBID slotName)
        {
            return parts.Find((part) => part.Contains(slotName));
        }

        public void SendChange(ArmatureRegistry.RegistryChange change, DBRegistry.DBID slotName)
        {
            UnityArmatureMeshPart part = GetPartWith(slotName);

            if (change == ArmatureRegistry.RegistryChange.DrawOrder && part.IsSingle && parts.Count != 1)
            {
                Combine();
            }
            else
            {
                part.SendChange(change, slotName);
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
    }
}