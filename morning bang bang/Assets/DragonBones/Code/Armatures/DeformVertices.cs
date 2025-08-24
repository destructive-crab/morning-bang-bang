using System.Collections.Generic;
namespace DragonBones
{
    public class DeformVertices : DBObject
    {
        public bool verticesDirty;
        
        public readonly List<float> vertices = new List<float>();
        public readonly List<Bone> bones = new List<Bone>();
        
        public VerticesData verticesData;

        public override void OnReleased()
        {
            verticesDirty = false;
            vertices.Clear();
            bones.Clear();
            verticesData = null;
        }

        public void Init(VerticesData verticesDataValue, Armature armature)
        {
            verticesData = verticesDataValue;

            if (verticesData != null)
            {
                int vertexCount = 0;
                
                if (verticesData.weight != null)
                {
                    vertexCount = verticesData.weight.count * 2;
                }
                else
                {
                    vertexCount = verticesData.data.intArray[verticesData.offset + (int)BinaryOffset.MeshVertexCount] * 2;
                }

                verticesDirty = true;
                vertices.ResizeList(vertexCount);
                bones.Clear();
                
                for (int i = 0, l = vertices.Count; i < l; ++i)
                {
                    vertices[i] = 0.0f;
                }

                if (verticesData.weight != null)
                {
                    for (int i = 0, l = verticesData.weight.bones.Count; i < l; ++i)
                    {
                        Bone bone = DB.Registry.GetBone(DB.Registry.SearchAtArmature(armature.ID, verticesData.weight.bones[i].name));
                        bones.Add(bone);
                    }
                }
            }
            else
            {
                verticesDirty = false;
                vertices.Clear();
                bones.Clear();
                verticesData = null;
            }
        }

        public bool AreBonesDirty()
        {
            foreach (var bone in bones)
            {
                if (bone != null && bone._childrenTransformDirty)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
