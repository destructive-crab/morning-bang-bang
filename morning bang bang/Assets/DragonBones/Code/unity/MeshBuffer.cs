using System;
using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    public class MeshBuffer : IDisposable
    {
        public readonly List<UnitySlot> combineSlots = new List<UnitySlot>();
        public string name;
        public Mesh sharedMesh;
        public int vertexCount;
        public Vector3[] rawVertextBuffers;
        public Vector2[] uvBuffers;
        public Vector3[] vertexBuffers;
        public Color32[] color32Buffers;
        public int[] triangleBuffers;

        public bool VertexDirty;
        public bool ZOrderDirty;
        public bool enabled;

        public static Mesh GenerateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            mesh.MarkDynamic();

            return mesh;
        }

        private static int CompareSlots(Slot a, Slot b)
        {
            if(a._zOrder > b._zOrder)
            {
                return 1;
            }

            if(a._zOrder < b._zOrder)
            {
                return -1;
            }

            return 0;
        }

        public void Dispose()
        {
            if (sharedMesh != null)
            {
                DBUnityFactory.UnityFactoryHelper.DestroyUnityObject(sharedMesh);
            }

            combineSlots.Clear();
            name = string.Empty;
            sharedMesh = null;
            vertexCount = 0;
            rawVertextBuffers = null;
            uvBuffers = null;
            vertexBuffers = null;
            color32Buffers = null;
            VertexDirty = false;
            enabled = false;
        }

        public void Clear()
        {
            if (sharedMesh != null)
            {
                sharedMesh.Clear();
                sharedMesh.uv = null;
                sharedMesh.vertices = null;
                sharedMesh.normals = null;
                sharedMesh.triangles = null;
                sharedMesh.colors32 = null;
            }

            name = string.Empty;
        }

        public void CombineMeshes(CombineInstance[] combines)
        {
            if (sharedMesh == null)
            {
                sharedMesh = GenerateMesh();
            }

            sharedMesh.CombineMeshes(combines);

            //
            uvBuffers = sharedMesh.uv;
            rawVertextBuffers = sharedMesh.vertices;
            vertexBuffers = sharedMesh.vertices;
            color32Buffers = sharedMesh.colors32;
            triangleBuffers = sharedMesh.triangles;

            vertexCount = vertexBuffers.Length;
            //
            if (color32Buffers == null || color32Buffers.Length != vertexCount)
            {
                color32Buffers = new Color32[vertexCount];
            }
        }

        public void InitMesh()
        {
            if (vertexBuffers != null)
            {
                vertexCount = vertexBuffers.Length;
            }
            else
            {
                vertexCount = 0;
            }

            if (color32Buffers == null || color32Buffers.Length != vertexCount)
            {
                color32Buffers = new Color32[vertexCount];
            }

            sharedMesh.vertices = vertexBuffers;// Must set vertices before uvs.
            sharedMesh.uv = uvBuffers;
            sharedMesh.colors32 = color32Buffers;
            sharedMesh.triangles = triangleBuffers;
            sharedMesh.RecalculateBounds();
            

            enabled = true;
        }

        public void UpdateVertices()
        {
            sharedMesh.vertices = vertexBuffers;
            sharedMesh.RecalculateBounds();
        }

        public void UpdateColors()
        {
            sharedMesh.colors32 = color32Buffers;
        }

        public void UpdateOrder()
        {
            combineSlots.Sort(CompareSlots);

            var index = 0;
            var newVerticeIndex = 0;
            var oldVerticeOffset = 0;

            var newUVs = new Vector2[vertexCount];
            var newVertices = new Vector3[vertexCount];
            var newColors = new Color32[vertexCount];
            CombineInstance[] combines = new CombineInstance[combineSlots.Count];
            for (int i = 0; i < combineSlots.Count; i++)
            {
                var slot = combineSlots[i];
                oldVerticeOffset = slot._verticeOffset;

                //Reassign
                slot._verticeOrder = i;
                slot._verticeOffset = newVerticeIndex;
                
                CombineInstance combineInstance = new CombineInstance();
                slot._meshBuffer.InitMesh();
                combineInstance.mesh = slot._meshBuffer.sharedMesh;

                combines[i] = combineInstance;
                
                var zspace = (slot._armature.Display as UnityEngineArmatureDisplay).zSpace;
                for (int j = 0; j < slot._meshBuffer.vertexCount; j++)
                {
                    index = oldVerticeOffset + j;
                    newUVs[newVerticeIndex] = this.uvBuffers[index];
                    newVertices[newVerticeIndex] = this.vertexBuffers[index];
                    newColors[newVerticeIndex] = this.color32Buffers[index];

                    newVertices[newVerticeIndex].z = -slot._verticeOrder * (zspace + UnitySlot.Z_OFFSET);

                    newVerticeIndex++;
                } 
            }

            //
            sharedMesh.Clear();
            sharedMesh.CombineMeshes(combines);
            //
            uvBuffers = newUVs;
            vertexBuffers = newVertices;
            color32Buffers = newColors;

            triangleBuffers = sharedMesh.triangles;

            InitMesh();
        }
    }
}