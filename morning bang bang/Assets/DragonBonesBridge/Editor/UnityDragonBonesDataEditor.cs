using DragonBones;
using UnityEditor;
using UnityEngine;

namespace DragonBonesBridge.Editor
{
    [CustomEditor(typeof(UnityDragonBonesData))]
    public class UnityDragonBonesDataEditor : UnityEditor.Editor
    {
        private UnityDragonBonesData dbData;

        private SerializedProperty dataName;
        private SerializedProperty mainJson;
        private SerializedProperty atlases;

        private void OnEnable()
        {
            dbData = target as UnityDragonBonesData;

            dataName = serializedObject.FindProperty("dataName");
            mainJson = serializedObject.FindProperty("dragonBonesJSON");
            atlases = serializedObject.FindProperty("textureAtlas");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(dataName);
            EditorGUILayout.PropertyField(mainJson);
            EditorGUILayout.PropertyField(atlases);
            
            foreach (UnityDragonBonesData.TextureAtlas atlas in dbData.textureAtlas)
            {
                if (atlas.material == null)
                {
                    if (GUILayout.Button($"Create Material for {atlas.texture.name}"))
                    {
                        Shader shader = Shader.Find("Sprites/Default");
                        Material material = new Material(shader);
                        material.name = atlas.texture.name + "_Mat";
                        material.mainTexture = atlas.texture;

                        string path = AssetDatabase.GetAssetPath(atlas.textureAtlasJSON);
                        
                        AssetDatabase.CreateAsset(material, path.Substring(0, path.Length-5) + "_Mat.mat");
                        AssetDatabase.SaveAssets();
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        } 
    }
}