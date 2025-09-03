using System;
using UnityEngine;

namespace DragonBones
{
    public sealed class UnityDBFactory : DBFactory
    {
        internal static class Helper
        {
            internal static Material GenerateMaterial(string shaderName, string materialName, Texture texture)
            {
                Shader shader = Shader.Find(shaderName);
                Material material = new Material(shader);
                material.name = materialName;
                material.mainTexture = texture;

                return material;
            }

            internal static string CheckResourcesPath(string path)
            {
                int index = path.LastIndexOf("Resources", StringComparison.Ordinal);
                
                if (index > 0)
                {
                    path = path.Substring(index + 10);
                }

                index = path.LastIndexOf(".", StringComparison.Ordinal);
                
                if (index > 0)
                {
                    path = path.Substring(0, index);
                }

                return path;
            }

            internal static string GetTextureAtlasImagePath(string textureAtlasJSONPath, string textureAtlasImageName)
            {
                var index = textureAtlasJSONPath.LastIndexOf("Resources", StringComparison.Ordinal);
                if (index > 0)
                {
                    textureAtlasJSONPath = textureAtlasJSONPath.Substring(index + 10);
                }

                index = textureAtlasJSONPath.LastIndexOf("/", StringComparison.Ordinal);

                string textureAtlasImagePath = textureAtlasImageName;
                if (index > 0)
                {
                    textureAtlasImagePath = textureAtlasJSONPath.Substring(0, index + 1) + textureAtlasImageName;
                }

                index = textureAtlasImagePath.LastIndexOf(".", StringComparison.Ordinal);
                if (index > 0)
                {
                    textureAtlasImagePath = textureAtlasImagePath.Substring(0, index);
                }

                return textureAtlasImagePath;
            }

            internal static void DestroyUnityObject(UnityEngine.Object obj)
            {
                if (obj == null) { return; }

#if UNITY_EDITOR
                UnityEngine.Object.DestroyImmediate(obj);
#else
                UnityEngine.Object.Destroy(obj);
#endif
            }

        }
    }
}