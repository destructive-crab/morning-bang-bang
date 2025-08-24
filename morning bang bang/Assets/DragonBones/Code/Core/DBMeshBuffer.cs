using UnityEngine;

namespace DragonBones
{
    public class DBMeshBuffer
    {
        public int VertexCount => vertexBuffer.Length;
        public Mesh GeneratedMesh;
        
        public Vector3[] rawVertexBuffer;
        
        public Vector3[] vertexBuffer;
        public Vector2[] uvBuffer;
        public Color32[] color32Buffer;
        public int[] triangleBuffer;

        public Material Material;

        public Mesh GenerateMesh()
        {
            GeneratedMesh = UnityDBFactory.GetEmptyMesh();
            GeneratedMesh.vertices = vertexBuffer;
            GeneratedMesh.uv = uvBuffer;
            GeneratedMesh.colors32 = color32Buffer;
            GeneratedMesh.triangles = triangleBuffer;

            return GeneratedMesh;
        }
    }
}