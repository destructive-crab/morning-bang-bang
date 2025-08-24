﻿using UnityEngine;

namespace DragonBones
{
    [System.Serializable]
    public class UnityDragonBonesData : ScriptableObject
    {
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

        public void RemoveFromCash(bool disposeData =true)
        {
            DB.Kernel.DataStorage.RemoveDragonBonesData(dataName, disposeData);
            if(textureAtlas != null)
            {
                foreach(TextureAtlas ta in textureAtlas)
                {
                    if(ta != null && ta.texture != null)
                    {
                        DB.Kernel.DataStorage.RemoveTextureAtlasData(ta.texture.name,disposeData);
                    }
                }
            }
        }
    }
}
