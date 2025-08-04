using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MothDIed.Core.GameObjects.Pool;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DragonBones
{
    public sealed class DBUnityFactory : DBFactory
    {
        internal const string defaultShaderName = "Sprites/Default";
        internal const string defaultUIShaderName = "UI/Default";
        
        private GameObjectPool<UnityEngineArmatureDisplay> displaysPool;

        private bool _isUGUI = false;

        public async UniTask InitializeFactory()
        {
            displaysPool = new GameObjectPool<UnityEngineArmatureDisplay>(new GameObjectPool<UnityEngineArmatureDisplay>.Config<UnityEngineArmatureDisplay>("Dragon Bones Armature Displays"));
            
            displaysPool.PoolConfiguration
                .SetSize(64)
                .IsExpandable(true)
                .IsPersistent(true)
                .SetFabric(new PoolFabric(true, false))
                .SetPrefab(null); //means pool will be filled with empty GO with UnityEngineArmatureDisplay component

            await displaysPool.WarmAsync();
        }

        public override TextureAtlasData BuildTextureAtlasData(TextureAtlasData textureAtlasData, object textureAtlas)
        {
            if (textureAtlasData != null)
            {
                if (textureAtlas != null)
                {
                    ((UnityTextureAtlasData)textureAtlasData).uiTexture = (textureAtlas as UnityDragonBonesData.TextureAtlas).uiMaterial;
                    ((UnityTextureAtlasData)textureAtlasData).texture = (textureAtlas as UnityDragonBonesData.TextureAtlas).material;
                }
            }
            else
            {
                textureAtlasData = DBObject.BorrowObject<UnityTextureAtlasData>();
            }

            return textureAtlasData;
        }

        /// <summary>
        /// Create a armature from cached DragonBonesData instances and TextureAtlasData instances, then use the {@link #clock} to update it.
        /// The difference is that the armature created by {@link #buildArmature} is not WorldClock instance update.
        /// </summary>
        /// <param name="armatureName">The armature data name</param>
        /// <param name="dragonBonesName">The cached name of the DragonBonesData instance (If not set, all DragonBonesData instances are retrieved, and when multiple DragonBonesData instances contain a the same name armature data, it may not be possible to accurately create a specific armature)</param>
        /// <param name="skinName">The skin name, you can set a different ArmatureData name to share it's skin data (If not set, use the default skin data)</param>
        /// <param name="textureAtlasName">The textureAtlas name</param>
        /// <param name="isUGUI">isUGUI default is false</param>
        /// <returns>The armature display container.</returns>
        /// <version> DragonBones 4.5</version>
        /// <language>en_US</language>

        public UnityEngineArmatureDisplay BuildArmatureComponent(string armatureName, string dragonBonesName = "", IEngineArmatureDisplay display = null, 
            string skinName = "", string textureAtlasName = "", bool isUGUI = false)
        {
            _isUGUI = isUGUI;
            Armature armature = BuildArmature(armatureName, dragonBonesName, skinName, textureAtlasName, display);

            if (armature != null)
            {
                DBInitial.Kernel.Clock.Add(armature);

                return armature.Display as UnityEngineArmatureDisplay;
            }

            return null;
        }

        public static void RefreshTextureAtlas(UnityTextureAtlasData textureAtlasData, bool isUGUI,
            bool isEditor = false)
        {
            Material material = null;
            if (isUGUI && textureAtlasData.uiTexture == null)
            {
                if (isEditor)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        material = AssetDatabase.LoadAssetAtPath<Material>(textureAtlasData.imagePath + "_UI_Mat.mat");
                    }
#endif
                }
                else
                {
                    material = Resources.Load<Material>(textureAtlasData.imagePath + "_UI_Mat");
                }

                if (material == null)
                {
                    Texture2D textureAtlas = null;

                    if (isEditor)
                    {
#if UNITY_EDITOR
                        if (!Application.isPlaying)
                        {
                            textureAtlas =
                                AssetDatabase.LoadAssetAtPath<Texture2D>(textureAtlasData.imagePath + ".png");
                        }
#endif
                    }
                    else
                    {
                        textureAtlas = Resources.Load<Texture2D>(textureAtlasData.imagePath);
                    }

                    material = UnityFactoryHelper.GenerateMaterial(defaultUIShaderName, textureAtlas.name + "_UI_Mat",
                        textureAtlas);
                    if (textureAtlasData.width < 2)
                    {
                        textureAtlasData.width = (uint)textureAtlas.width;
                    }

                    if (textureAtlasData.height < 2)
                    {
                        textureAtlasData.height = (uint)textureAtlas.height;
                    }

                    textureAtlasData._disposeEnabled = true;
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        string path = AssetDatabase.GetAssetPath(textureAtlas);
                        path = path.Substring(0, path.Length - 4);
                        AssetDatabase.CreateAsset(material, path + "_UI_Mat.mat");
                        AssetDatabase.SaveAssets();
                    }
#endif
                }

                textureAtlasData.uiTexture = material;
            }
            else if (!isUGUI && textureAtlasData.texture == null)
            {
                if (isEditor)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        material = AssetDatabase.LoadAssetAtPath<Material>(textureAtlasData.imagePath + "_Mat.mat");
                    }
#endif
                }
                else
                {
                    material = Resources.Load<Material>(textureAtlasData.imagePath + "_Mat");
                }

                if (material == null)
                {
                    Texture2D textureAtlas = null;
                    if (isEditor)
                    {
#if UNITY_EDITOR
                        if (!Application.isPlaying)
                        {
                            textureAtlas =
                                AssetDatabase.LoadAssetAtPath<Texture2D>(textureAtlasData.imagePath + ".png");
                        }
#endif
                    }
                    else
                    {
                        textureAtlas = Resources.Load<Texture2D>(textureAtlasData.imagePath);
                    }

                    material = UnityFactoryHelper.GenerateMaterial(defaultShaderName, textureAtlas.name + "_Mat",
                        textureAtlas);
                    if (textureAtlasData.width < 2)
                    {
                        textureAtlasData.width = (uint)textureAtlas.width;
                    }

                    if (textureAtlasData.height < 2)
                    {
                        textureAtlasData.height = (uint)textureAtlas.height;
                    }

                    textureAtlasData._disposeEnabled = true;
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        string path = AssetDatabase.GetAssetPath(textureAtlas);
                        path = path.Substring(0, path.Length - 4);
                        AssetDatabase.CreateAsset(material, path + "_Mat.mat");
                        AssetDatabase.SaveAssets();
                    }
#endif
                }

                textureAtlasData.texture = material;
            }
        }

        public override void Clear(bool disposeData = true)
        {
            _isUGUI = false;
        }

        public void RefreshAllTextureAtlas(UnityEngineArmatureDisplay unityEngineArmature)
        {
            foreach (List<TextureAtlasData> textureAtlasDatas in DBInitial.Kernel.DataStorage.GetAllTextureAtlases().Values)
            {
                foreach (var atlas in textureAtlasDatas)
                {
                    var unityAtlas = (UnityTextureAtlasData)atlas;
                    RefreshTextureAtlas(unityAtlas, unityEngineArmature.isUGUI);
                }
            }
        }

        public override void ReplaceDisplay(Slot slot, DisplayData displayData, int displayIndex = -1)
        {
            //UGUI Display Object and Normal Display Object cannot be replaced with each other
            if (displayData.type == DisplayType.Image || displayData.type == DisplayType.Mesh)
            {
                var dataName = displayData.parent.parent.parent.name;
                var textureData = DBInitial.Kernel.DataStorage.GetTextureData(dataName, displayData.path);
                if (textureData != null)
                {
                    var textureAtlasData = textureData.parent as UnityTextureAtlasData;

                    var oldIsUGUI = (slot.Armature.Display as UnityEngineArmatureDisplay).isUGUI;

                    if ((oldIsUGUI && textureAtlasData.uiTexture == null) ||
                        (!oldIsUGUI && textureAtlasData.texture == null))
                    {
                        DBLogger.LogWarning(
                            "ugui display object and normal display object cannot be replaced with each other");
                        return;
                    }
                }
            }

            base.ReplaceDisplay(slot, displayData, displayIndex);
        }

        public void ReplaceSlotDisplay(
            string dragonBonesName, string armatureName, string slotName, string displayName,
            Slot slot, Texture2D texture, Material material = null,
            bool isUGUI = false, int displayIndex = -1)
        {
            ArmatureData armatureData = DBInitial.Kernel.DataStorage.GetArmatureData(armatureName, dragonBonesName);
            
            if (armatureData == null || armatureData.defaultSkin == null) { return; }

            DisplayData previousDisplayData = armatureData.defaultSkin.GetDisplay(slotName, displayName);

            if (previousDisplayData == null || !((previousDisplayData is ImageDisplayData) || (previousDisplayData is MeshDisplayData)))
            {
                return;
            }

            TextureData previousTextureData = null;
            
            if (previousDisplayData is ImageDisplayData) { previousTextureData = (previousDisplayData as ImageDisplayData).texture; }
            else { previousTextureData = (previousDisplayData as MeshDisplayData).texture; }

            UnityTextureData newTextureData = new UnityTextureData();
            
            newTextureData.CopyFrom(previousTextureData);
            newTextureData.rotated = false;
            newTextureData.region.x = 0.0f;
            newTextureData.region.y = 0.0f;
            newTextureData.region.width = texture.width;
            newTextureData.region.height = texture.height;
            newTextureData.frame = newTextureData.region;
            newTextureData.name = previousTextureData.name;
            newTextureData.parent = new UnityTextureAtlasData();
            newTextureData.parent.width = (uint)texture.width;
            newTextureData.parent.height = (uint)texture.height;
            newTextureData.parent.scale = previousTextureData.parent.scale;

            //
            if (material == null)
            {
                if (isUGUI)
                {
                    material = UnityFactoryHelper.GenerateMaterial(defaultUIShaderName, texture.name + "_UI_Mat",
                        texture);
                }
                else
                {
                    material = UnityFactoryHelper.GenerateMaterial(defaultShaderName, texture.name + "_Mat", texture);
                }
            }

            if (isUGUI)
            {
                (newTextureData.parent as UnityTextureAtlasData).uiTexture = material;
            }
            else
            {
                (newTextureData.parent as UnityTextureAtlasData).texture = material;
            }

            material.mainTexture = texture;

            DisplayData newDisplayData = null;
            
            if (previousDisplayData is ImageDisplayData)
            {
                newDisplayData = new ImageDisplayData();
                newDisplayData.type = previousDisplayData.type;
                newDisplayData.name = previousDisplayData.name;
                newDisplayData.path = previousDisplayData.path;
                newDisplayData.DBTransform.CopyFrom(previousDisplayData.DBTransform);
                newDisplayData.parent = previousDisplayData.parent;
                (newDisplayData as ImageDisplayData).pivot.CopyFrom((previousDisplayData as ImageDisplayData).pivot);
                (newDisplayData as ImageDisplayData).texture = newTextureData;
            }
            else if (previousDisplayData is MeshDisplayData meshDisplayData)
            {
                newDisplayData = new MeshDisplayData();
                newDisplayData.type = previousDisplayData.type;
                newDisplayData.name = previousDisplayData.name;
                newDisplayData.path = previousDisplayData.path;
                newDisplayData.DBTransform.CopyFrom(previousDisplayData.DBTransform);
                newDisplayData.parent = previousDisplayData.parent;
                
                ((MeshDisplayData)newDisplayData).texture = newTextureData;
                ((MeshDisplayData)newDisplayData).vertices.inheritDeform = meshDisplayData.vertices.inheritDeform;
                ((MeshDisplayData)newDisplayData).vertices.offset = meshDisplayData.vertices.offset;
                ((MeshDisplayData)newDisplayData).vertices.data = meshDisplayData.vertices.data;
                ((MeshDisplayData)newDisplayData).vertices.weight = meshDisplayData.vertices.weight;
            }

            ReplaceDisplay(slot, newDisplayData, displayIndex);
        }

        protected override Armature BuildArmatureProxy(BuildArmaturePackage dataPackage)
        {
            Armature armature = DBObject.BorrowObject<Armature>();
            UnityEngineArmatureDisplay armatureDisplay = null;

            if (dataPackage.display == null)
            {
                armatureDisplay = displaysPool.Get();
            }
            else
            {
                armatureDisplay = dataPackage.display as UnityEngineArmatureDisplay;
                armatureDisplay.Armature?.ReturnToPool();
                
                (dataPackage.display as IEngineArmatureDisplay).DBClear();
            }

            armatureDisplay.Armature = armature;
            armature.Init(dataPackage.armatureData, armatureDisplay);

            return armature;
        }

        protected override Armature BuildChildArmature(BuildArmaturePackage dataPackage, Slot slot, DisplayData displayData)
        {
            string childDisplayName = slot.SlotData.name + " (" + displayData.path + ")"; 
            UnityEngineArmatureDisplay proxy = slot.Armature.Display as UnityEngineArmatureDisplay;
            Transform childTransform = proxy.transform.Find(childDisplayName);

            Armature childArmature = null;
            
            if (childTransform == null)
            {
                if (dataPackage != null)
                {
                    childArmature = BuildArmature(displayData.path, dataPackage.dataName);
                }
                else
                {
                    childArmature = BuildArmature(displayData.path, displayData.parent.parent.parent.name);
                }
            }
            else
            {
                if (dataPackage != null)
                {
                    childArmature = BuildArmatureComponent(displayData.path, dataPackage != null ? dataPackage.dataName : "", childTransform.gameObject.GetComponent<UnityEngineArmatureDisplay>(), null, dataPackage.textureAtlasName).Armature;
                }
                else
                {
                    childArmature =
                        BuildArmatureComponent(displayData.path, null, childTransform.gameObject.GetComponent<UnityEngineArmatureDisplay>(), null, null).Armature;
                }
            }

            if (childArmature == null)
            {
                return null;
            }

            UnityEngineArmatureDisplay childArmatureDisplay = childArmature.Display as UnityEngineArmatureDisplay;
            childArmatureDisplay.isUGUI = proxy.GetComponent<UnityEngineArmatureDisplay>().isUGUI;
            childArmatureDisplay.name = childDisplayName;
            childArmatureDisplay.transform.SetParent(proxy.transform, false);
            childArmatureDisplay.gameObject.SetActive(false);
            return childArmature;
        }

        protected override Slot BuildSlot(BuildArmaturePackage dataPackage, SlotData slotData, Armature armature)
        {
            UnitySlot slot = DBObject.BorrowObject<UnitySlot>();
            
            UnityEngineArmatureDisplay armatureDisplay = armature.Display as UnityEngineArmatureDisplay;
            
            Transform slotTransform = armatureDisplay.transform.Find(slotData.name);
            GameObject slotGameObject = slotTransform == null ? null : slotTransform.gameObject;
            
            bool isNeedIngoreCombineMesh = false;
            
            if (slotGameObject == null)
            {
                slotGameObject = new GameObject(slotData.name);
            }
            else
            {
                if (slotGameObject.hideFlags == HideFlags.None)
                {
                    var combineMeshs = (armature.Display as UnityEngineArmatureDisplay).GetComponent<UnityCombineMeshes>();
                    if (combineMeshs != null)
                    {
                        isNeedIngoreCombineMesh = !combineMeshs.slotNames.Contains(slotData.name);
                    }
                }
            }

            slot.Init(slotData, armature, slotGameObject, slotGameObject);

            if (isNeedIngoreCombineMesh)
            {
                slot.DisallowCombineMesh();
            }

            return slot;
        }
        
        internal static class UnityFactoryHelper
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

            internal static string GetTextureAtlasNameByPath(string textureAtlasJSONPath)
            {
                string name = string.Empty;
                int index = textureAtlasJSONPath.LastIndexOf("/", StringComparison.Ordinal) + 1;
                int lastIdx = textureAtlasJSONPath.LastIndexOf("_tex", StringComparison.Ordinal);

                if (lastIdx > -1)
                {
                    if (lastIdx > index)
                    {
                        name = textureAtlasJSONPath.Substring(index, lastIdx - index);
                    }
                    else
                    {
                        name = textureAtlasJSONPath.Substring(index);
                    }
                }
                else
                {
                    if (index > -1)
                    {
                        name = textureAtlasJSONPath.Substring(index);
                    }

                }

                return name;
            }

            internal static void DestroyUnityObject(UnityEngine.Object obj)
            {
                if (obj == null)
                {
                    return;
                }

#if UNITY_EDITOR
                UnityEngine.Object.DestroyImmediate(obj);
#else
            UnityEngine.Object.Destroy(obj);
#endif
            }
        }
    }
}