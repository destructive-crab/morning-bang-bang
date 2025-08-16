using System;
using UnityEngine;

namespace DragonBones
{
    public sealed class MeshBuffer : IDisposable
    {
        public string Name;

        public Mesh sharedMesh { get; private set; }

        public int VertexCount => vertexBuffer.Length;

        public Vector3[] rawVertexBuffer;

        public Material Material;
        
        public Vector2[] uvBuffer;
        public Vector3[] vertexBuffer;
        public Color32[] color32Buffer;
        public int[] triangleBuffer;

        public static Mesh GetEmptyMesh()
        {
            Mesh mesh = new Mesh();
            mesh.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            mesh.MarkDynamic();

            return mesh;
        }

        public Mesh InitMesh()
        {
            if (sharedMesh != null)
            {
                sharedMesh.Clear();
            }
            else
            {
                sharedMesh = GetEmptyMesh();
            }
            
            return UpdateMesh();
        }
        public Mesh UpdateMesh()
        {
            sharedMesh.vertices = vertexBuffer;
            sharedMesh.uv = uvBuffer;
            sharedMesh.colors32 = color32Buffer;
            sharedMesh.triangles = triangleBuffer;

            return sharedMesh;
        }
        
        public void Clear()
        {
            Name = string.Empty;
        }

        public void Dispose()
        {
            if (sharedMesh != null)
            {
                DBUnityFactory.Helper.DestroyUnityObject(sharedMesh);
            }

            Name = string.Empty;
            sharedMesh = null;
            rawVertexBuffer = null;
            uvBuffer = null;
            vertexBuffer = null;
            color32Buffer = null;
        }
    }
}
