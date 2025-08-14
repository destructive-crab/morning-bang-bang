using System;
using UnityEngine;

namespace DragonBones
{
    public sealed class MeshBuffer : IDisposable
    {
        public string name;
        
        public Mesh sharedMesh;
        public int vertexCount;
        
        public Vector3[] rawVertexBuffers;
        public Vector2[] uvBuffers;
        public Vector3[] vertexBuffers;
        public Color32[] color32Buffers;
        public int[] triangleBuffers;

        public bool VertexDirty;
        public bool ZOrderDirty;
        
        public bool enabled;

        private static int CompareSlots(Slot a, Slot b)
        {
            if(a.ZOrder.Value > b.ZOrder.Value) { return 1; }
            if(a.ZOrder.Value < b.ZOrder.Value) { return -1; }
            return 0;
        }

        public static Mesh GetNewMesh()
        {
            Mesh mesh = new Mesh();
            mesh.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            mesh.MarkDynamic();

            return mesh;
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

        public void Dispose()
        {
            if (sharedMesh != null)
            {
                DBUnityFactory.Helper.DestroyUnityObject(sharedMesh);
            }

            name = string.Empty;
            sharedMesh = null;
            vertexCount = 0;
            rawVertexBuffers = null;
            uvBuffers = null;
            vertexBuffers = null;
            color32Buffers = null;
            VertexDirty = false;
            enabled = false;
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
    }
}