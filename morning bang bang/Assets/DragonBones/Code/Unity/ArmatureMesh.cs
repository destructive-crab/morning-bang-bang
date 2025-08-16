using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DragonBones
{
    public sealed class ArmatureMesh
    {
        public bool IsCombined { get; private set; } = false;

        public readonly UnityEngineArmatureDisplay BelongsTo;

        private readonly List<ArmatureMeshPart> parts = new();

        private Mesh sharedMesh;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        public ArmatureMesh(UnityEngineArmatureDisplay belongsTo)
        {
            BelongsTo = belongsTo;

            meshFilter = BelongsTo.gameObject.AddComponent<MeshFilter>();
            meshRenderer = BelongsTo.gameObject.AddComponent<MeshRenderer>();
        }

        public void Update()
        {
            List<Vector3> vertices = new();
            foreach (ArmatureMeshPart meshPart in parts)
            {
                meshPart.UpdateVertices();
                vertices.AddRange(meshPart.sharedMesh.vertices);
            }
            
            sharedMesh.vertices = vertices.ToArray();
            sharedMesh.RecalculateBounds();
        }
        
        public void Combine()
        {
            CollectParts();
            
            List<Vector3> vertices = new();
            List<Vector2> uvs = new();
            List<int[]> triangles = new();
            
            foreach (ArmatureMeshPart meshPart in parts)
            {
                triangles.Add(meshPart.sharedMesh.triangles);
              
                for (var i = 0; i < triangles.Last().Length; i++)
                {
                    triangles.Last()[i] += vertices.Count;
                }
                
                vertices.AddRange(meshPart.sharedMesh.vertices);
                uvs.AddRange(meshPart.sharedMesh.uv);
            }

            sharedMesh = MeshBuffer.GetEmptyMesh();

            sharedMesh.vertices = vertices.ToArray();
            sharedMesh.uv = uvs.ToArray();

            sharedMesh.subMeshCount= triangles.Count;

            List<Material> materials = new();
            for (var i = 0; i < triangles.Count; i++)
            {
                int[] triangle = triangles[i];
                sharedMesh.SetTriangles(triangle, i);
                materials.Add(parts[i].Material);
            }

            sharedMesh.RecalculateBounds();
            meshFilter.sharedMesh = sharedMesh;
            meshRenderer.materials = materials.ToArray();
            
            IsCombined = true;
        }
        private void CollectParts()
        {
            Material currentCombiningWith = (BelongsTo.Armature.Structure.Slots[0] as UnitySlot).CurrentTextureAtlasData.texture;
            List<UnitySlot> currentCombine = new();
            
            foreach (Slot slot in BelongsTo.Armature.Structure.Slots)
            {
                if(slot.IsDisplayingChildArmature()) continue;

                UnitySlot unitySlot = slot as UnitySlot;

                if (unitySlot.CurrentTextureAtlasData.texture == currentCombiningWith)
                {
                    currentCombine.Add(unitySlot);
                }
                else
                {
                    parts.Add(new ArmatureMeshPart(BelongsTo));
                    parts.Last().Build(currentCombine.ToArray(), currentCombiningWith);

                    currentCombiningWith = unitySlot.CurrentTextureAtlasData.texture;
                    currentCombine.Clear();
                    currentCombine.Add(unitySlot);
                }
            }
            parts.Add(new ArmatureMeshPart(BelongsTo));
            parts.Last().Build(currentCombine.ToArray(), currentCombiningWith);
        }

        public bool Contains(string slotName)
        {
            return parts.Any((part) => part.Contains(slotName));
        }
    }
}
