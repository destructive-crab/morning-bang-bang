using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DragonBones
{
    public class UnityDataLoader
    {
        /// <summary>
        /// Parse the UnityDragonBonesData to a DragonBonesData instance and cache it 
        /// </summary>
        /// <param name="data">The UnityDragonBonesData data</param>
        /// <param name="isUGUI">is UGUI</param>
        /// <param name="armatureScale">The armature scale</param>
        /// <param name="texScale">The texture scale</param>
        /// <returns></returns>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public DragonBonesData LoadData(UnityDragonBonesData data, bool isUGUI = false, float armatureScale = 0.01f, float texScale = 1.0f)
        {
            DragonBonesData dragonBonesData = null;

            if (data.dragonBonesJSON != null)
            {
                dragonBonesData = LoadDragonBonesData(data.dragonBonesJSON, data.dataName, armatureScale);

                if (!string.IsNullOrEmpty(data.dataName) && dragonBonesData != null && data.textureAtlas != null)
                {
#if UNITY_EDITOR
                    bool isDirty = false;
                    if (!Application.isPlaying)
                    {
                        for (int i = 0; i < data.textureAtlas.Length; ++i)
                        {
                            if (isUGUI)
                            {
                                if (data.textureAtlas[i].uiMaterial == null)
                                {
                                    isDirty = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (data.textureAtlas[i].material == null)
                                {
                                    isDirty = true;
                                    break;
                                }
                            }
                        }
                    }
#endif

                    var textureAtlasDatas = DBInitial.Kernel.DataStorage.GetTextureAtlasData(data.dataName);
                    if (textureAtlasDatas != null)
                    {
                        for (int i = 0, l = textureAtlasDatas.Count; i < l; ++i)
                        {
                            if (i < data.textureAtlas.Length)
                            {
                                var textureAtlasData = textureAtlasDatas[i] as UnityTextureAtlasData;
                                var textureAtlas = data.textureAtlas[i];

                                textureAtlasData.uiTexture = textureAtlas.uiMaterial;
                                textureAtlasData.texture = textureAtlas.material;
#if UNITY_EDITOR
                                if (!Application.isPlaying)
                                {
                                    textureAtlasData.imagePath = AssetDatabase.GetAssetPath(textureAtlas.texture);
                                    textureAtlasData.imagePath = textureAtlasData.imagePath.Substring(0, textureAtlasData.imagePath.Length - 4);
                                    DBUnityFactory.RefreshTextureAtlas(textureAtlasData, isUGUI, true);
                                    if (isUGUI)
                                    {
                                        textureAtlas.uiMaterial = textureAtlasData.uiTexture;
                                    }
                                    else
                                    {
                                        textureAtlas.material = textureAtlasData.texture;
                                    }
                                }
#endif
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < data.textureAtlas.Length; ++i)
                        {
                            LoadTextureAtlasData(data.textureAtlas[i], data.dataName, texScale, isUGUI);
                        }
                    }

#if UNITY_EDITOR
                    if (isDirty)
                    {
                        AssetDatabase.Refresh();
                        EditorUtility.SetDirty(data);
                        AssetDatabase.SaveAssets();
                    }
#endif
                }
            }

            return dragonBonesData;
        }

        /// <summary>
        /// Parse the raw data to a DragonBonesData instance and cache it to the factory.
        /// </summary>
        /// <param name="dragonBonesJSONPath">The path of dragonBones data in Resources. (other forms of loading can be extended by themselves)</param>
        /// <param name="name">Specify a cache name for the instance so that the instance can be obtained through this name. (If not set, use the instance name instead)</param>
        /// <param name="scale">Specify a scaling value for all armatures. (Default does not scale)</param>
        /// <returns>DragonBonesData instance</returns>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public DragonBonesData LoadDragonBonesData(string dragonBonesJSONPath, string name = "", float scale = 0.01f)
        {
            dragonBonesJSONPath = DBUnityFactory.Helper.CheckResourcesPath(dragonBonesJSONPath);

            TextAsset dragonBonesJSON = Resources.Load<TextAsset>(dragonBonesJSONPath);

            DragonBonesData dragonBonesData = LoadDragonBonesData(dragonBonesJSON, name);

            return dragonBonesData;
        }

        /// <summary>
        /// Parse the json data to a DragonBonesData instance and cache it to the factory.
        /// </summary>
        /// <param name="dragonBonesJSON">The jsonData of dragonBones</param>
        /// <param name="name">Specify a cache name for the instance so that the instance can be obtained through this name. (If not set, use the instance name instead)</param>
        /// <param name="scale">Specify a scaling value for all armatures. (Default does not scale)</param>
        /// <returns>DragonBonesData instance</returns>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public DragonBonesData LoadDragonBonesData(TextAsset dragonBonesJSON, string name = "", float scale = 0.01f)
        {
            if (dragonBonesJSON == null)
            {
                return null;
            }
            
            if (!string.IsNullOrEmpty(name))
            {
                var existedData = DBInitial.Kernel.DataStorage.GetDragonBonesData(name);
                if (existedData != null)
                {
                    return existedData; //was already loaded
                }
            }

            DragonBonesData data = null;

            if (dragonBonesJSON.text == "DBDT")
            {
                BinaryDataParser.jsonParseDelegate = MiniJSON.Json.Deserialize;
                data = DBInitial.Kernel.DataStorage.ParseAndAddDragonBonesData(dragonBonesJSON.bytes, name, scale); // Unity default Scale Factor.
            }
            else
            {
                data = DBInitial.Kernel.DataStorage.ParseAndAddDragonBonesData((Dictionary<string, object>)MiniJSON.Json.Deserialize(dragonBonesJSON.text), name, scale); // Unity default Scale Factor.
            }

            return data;
        }
        /// <summary>
        /// Parse the textureAtlas json data to a UnityTextureAtlasData instance and cache it to the factory.
        /// </summary>
        /// <param name="textureAtlasJSONPath">The path of dragonBones data in Resources. (other forms of loading can be extended by themselves. use factory.ParseTextureAtlasData(JSONObject, Material))</param>
        /// <param name="name">Specify a cache name for the instance so that the instance can be obtained through this name. (If not set, use the instance name instead)</param>
        /// <param name="scale">Specify a scaling value for textureAtlas. (Default does not scale)</param>
        /// <param name="isUGUI"></param>
        /// <returns></returns>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>  
        public UnityTextureAtlasData LoadTextureAtlasData(string textureAtlasJSONPath, string name = "", float scale = 1.0f, bool isUGUI = false)
        {
            textureAtlasJSONPath = DBUnityFactory.Helper.CheckResourcesPath(textureAtlasJSONPath);

            TextAsset textureAtlasJSON = Resources.Load<TextAsset>(textureAtlasJSONPath);

            //
            if (textureAtlasJSON != null)
            {
                Dictionary<string, object> textureJSONData = (Dictionary<string, object>)MiniJSON.Json.Deserialize(textureAtlasJSON.text);
                UnityTextureAtlasData textureAtlasData = DBInitial.Kernel.DataStorage.ParseAndAddTextureAtlasData(textureJSONData, null, name, scale) as UnityTextureAtlasData;

                if (textureAtlasData != null)
                {
                    textureAtlasData.imagePath = DBUnityFactory.Helper.GetTextureAtlasImagePath(textureAtlasJSONPath, textureAtlasData.imagePath);

                    DBUnityFactory.RefreshTextureAtlas(textureAtlasData, isUGUI);
                }

                return textureAtlasData;
            }

            return null;
        }
        /// <summary>
        /// Parse the TextureAtlas to a UnityTextureAtlasData instance and cache it to the factory.
        /// </summary>
        /// <param name="textureAtlasJSONPath">The path of dragonBones data in Resources. (other forms of loading can be extended by themselves. use factory.ParseTextureAtlasData(JSONObject, Material))</param>
        /// <param name="name">Specify a cache name for the instance so that the instance can be obtained through this name. (If not set, use the instance name instead)</param>
        /// <param name="scale">Specify a scaling value for textureAtlas. (Default does not scale)</param>
        /// <param name="isUGUI"></param>
        /// <returns></returns>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>  
        public UnityTextureAtlasData LoadTextureAtlasData(UnityDragonBonesData.TextureAtlas textureAtlas, string name, float scale = 1.0f, bool isUGUI = false)
        {
            Dictionary<string, object> textureJSONData = (Dictionary<string, object>)MiniJSON.Json.Deserialize(textureAtlas.textureAtlasJSON.text);
            UnityTextureAtlasData textureAtlasData = DBInitial.Kernel.DataStorage.ParseAndAddTextureAtlasData(textureJSONData, null, name, scale) as UnityTextureAtlasData;

            if (textureJSONData.ContainsKey("width"))
            {
                textureAtlasData.width = uint.Parse(textureJSONData["width"].ToString());
            }

            if (textureJSONData.ContainsKey("height"))
            {
                textureAtlasData.height = uint.Parse(textureJSONData["height"].ToString());
            }

            if (textureAtlasData != null)
            {
                textureAtlasData.uiTexture = textureAtlas.uiMaterial;
                textureAtlasData.texture = textureAtlas.material;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    textureAtlasData.imagePath = AssetDatabase.GetAssetPath(textureAtlas.texture);
                    textureAtlasData.imagePath = textureAtlasData.imagePath.Substring(0, textureAtlasData.imagePath.Length - 4);
                    DBUnityFactory.RefreshTextureAtlas(textureAtlasData, isUGUI, true);
                    if (isUGUI)
                    {
                        textureAtlas.uiMaterial = textureAtlasData.uiTexture;
                    }
                    else
                    {
                        textureAtlas.material = textureAtlasData.texture;
                    }
                }
#endif
            }

            return textureAtlasData;
        }
    }
}