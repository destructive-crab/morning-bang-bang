using UnityEngine;

namespace DragonBones
{
    public class DBMeshBuffer : DBObject
    {
        public UnitySlot AttachedTo { get; private set; }
        public int DrawOrder;
        
        public int VertexCount => vertexBuffer.Length;
        public Mesh GeneratedMesh;
        
        public Vector3[] rawVertexBuffer;
        
        public Vector3[] vertexBuffer;
        public Vector2[] uvBuffer;
        public Color32[] color32Buffer;
        public int[] triangleBuffer;

        public Material Material;

        public void Init(UnitySlot slot) => AttachedTo = slot;
        
        public Mesh GenerateMesh()
        {
            if(GeneratedMesh == null)
            {
                GeneratedMesh = UnityDBFactory.GetEmptyMesh();
            }

            GeneratedMesh.vertices = vertexBuffer;
            GeneratedMesh.uv = uvBuffer;
            GeneratedMesh.colors32 = color32Buffer;
            GeneratedMesh.triangles = triangleBuffer;

            return GeneratedMesh;
        }

        public override void OnReleased()
        {
            AttachedTo = null;
            vertexBuffer = null;
            uvBuffer = null;
            color32Buffer = null;
            triangleBuffer = null;
            rawVertexBuffer = null;
            Material = null;
            
            if(GeneratedMesh != null)
            {
                GeneratedMesh.Clear();
            }
        }

        public void Clear()
        {
            vertexBuffer = null;
            uvBuffer = null;
            color32Buffer = null;
            triangleBuffer = null;
            rawVertexBuffer = null;
            Material = null;
            
            GeneratedMesh?.Clear();
        }
    }
}