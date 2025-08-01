using UnityEngine;

namespace DragonBones
{
    [System.Serializable]
    public class UnityDragonBonesData : ScriptableObject
    {
        public int PixelsPerUnit = 100;
        
        [System.Serializable]
        public class TextureAtlas
        {
            public TextAsset textureAtlasJSON; 
            public Texture2D texture;
            public Material material;
            public Material uiMaterial;
        }

        public string dataName;
        public TextAsset dragonBonesJSON;
        public TextureAtlas[] textureAtlas;

        public void RemoveFromFactory(bool disposeData =true)
        {
            DBUnityFactory.factory.RemoveDragonBonesData(dataName, disposeData);
            if(textureAtlas != null)
            {
                foreach(TextureAtlas ta in textureAtlas)
                {
                    if(ta != null && ta.texture != null)
                    {
                        DBUnityFactory.factory.RemoveTextureAtlasData(ta.texture.name,disposeData);
                    }
                }
            }
        }
    }
}
