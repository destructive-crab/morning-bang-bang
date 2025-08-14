using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MothDIed.Pool;
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
        
        private GameObjectPool<UnityEngineArmatureDisplay> unityArmatureDisplaysPool;
        
        private GameObjectPool<UnityEngineMeshSlotDisplay> unityMeshDisplaysPool;
        private GameObjectPool<UnityEngineChildArmatureSlotDisplay> unityChildArmatureDisplaysPool;

        public async UniTask InitializeFactory()
        {
            unityArmatureDisplaysPool = InitializeNewDBGOPool<UnityEngineArmatureDisplay>(64, "Armature Displays");

            unityMeshDisplaysPool = InitializeNewDBGOPool<UnityEngineMeshSlotDisplay>(128, "Mesh Displays");
            unityChildArmatureDisplaysPool = InitializeNewDBGOPool<UnityEngineChildArmatureSlotDisplay>(16, "Child Armature Displays");

            await unityArmatureDisplaysPool.WarmAsync();
            await unityMeshDisplaysPool.WarmAsync();
            await unityChildArmatureDisplaysPool.WarmAsync();
        }

        private GameObjectPool<TPool> InitializeNewDBGOPool<TPool>(ushort size, string name)
            where TPool : Component
        {
            GameObjectPool<TPool> pool = new GameObjectPool<TPool>(new GameObjectPool<TPool>.Config<TPool>("Dragon Bones " + name + " Pool"));
            
            pool.PoolConfiguration
                .SetSize(size)
                .IsExpandable(true)
                .IsPersistent(true)
                .SetFabric(new PoolFabric(true, false))
                .SetPrefab(null); //means pool will be filled with empty GO with UnityEngineArmatureDisplay component on them

            return pool;
        }

        public override void Clear(bool disposeData = true)
        {
            if (disposeData)
            {
                unityArmatureDisplaysPool.Dispose();
            }
        }
        
        /// <summary>
        /// Create a armature from cached DragonBonesData instances and TextureAtlasData instances, then use the {@link #clock} to update it.
        /// The difference is that the armature created by {@link #buildArmature} is not WorldClock instance update.
        /// </summary>
        /// <param name="armatureName">The armature data name</param>
        /// <param name="dragonBonesName">The cached name of the DragonBonesData instance (If not set, all DragonBonesData instances are retrieved, and when multiple DragonBonesData instances contain a the same name armature data, it may not be possible to accurately create a specific armature)</param>
        /// <param name="skinName">The skin name, you can set a different ArmatureData name to share it's skin data (If not set, use the default skin data)</param>
        /// <param name="textureAtlasName">The textureAtlas name</param>
        /// <returns>The armature display container.</returns>
        /// <language>en_US</language>
        public UnityEngineArmatureDisplay UnityCreateArmature(string armatureName, string dragonBonesName = "", IEngineArmatureDisplay display = null, 
            string skinName = "", string textureAtlasName = "")
        {
            Armature armature = BuildArmature(armatureName, dragonBonesName, skinName, textureAtlasName, display);
            return armature.Display as UnityEngineArmatureDisplay;
        }

        protected override IEngineChildArmatureSlotDisplay BuildChildArmatureDisplay(BuildArmaturePackage dataPackage,
            Slot forSlot, ChildArmatureDisplayData childArmatureData)
        {
            UnityEngineArmatureDisplay parentArmatureDisplay = forSlot.Armature.Display as UnityEngineArmatureDisplay;
            UnityEngineChildArmatureSlotDisplay childArmatureDisplay = unityChildArmatureDisplaysPool.Pick();
            
            UnityEngineArmatureDisplay armatureDisplay = UnityCreateArmature(childArmatureData.armature.name, dataPackage.DataName, childArmatureDisplay.ArmatureDisplay);

            childArmatureDisplay.Build(childArmatureData, forSlot as UnitySlot);
            childArmatureDisplay.Disable();
            
            return childArmatureDisplay;
        }

        protected override IEngineArmatureDisplay GetNewArmatureDisplayFor(Armature armature)
        {
            if (armature == null)
            {
                DBLogger.LogWarning($"No armature instance was provided for building display");
                return null;
            }
            if (armature.Display != null)
            {
                DBLogger.LogWarning($"Armature ({armature.Name}) already has display");
                return armature.Display;
            }
            
            CurLog().AddEntry("Pick", "Unity Armature Display");
            return unityArmatureDisplaysPool.Pick();
        }

        protected override Slot BuildSlot(BuildArmaturePackage dataPackage, SlotData slotData,
            List<DisplayData> displayDatas, Armature armature)
        {
            DBLogger.BLog.StartSection($"Build Slot {slotData.name}");
            
            DBLogger.BLog.AddEntry("Borrow", "Unity Slot", $"data: {slotData.name}; displays count: {displayDatas.Count}");
            
            UnitySlot slot = DBObject.BorrowObject<UnitySlot>();
            
            slot.StartSlotBuilding(slotData, armature);
            slot.StarUnitySlotBuilding(armature.Display as UnityEngineArmatureDisplay);
            
            bool isNeedIgnoreCombineMesh = false;

            foreach (DisplayData displayData in displayDatas)
            {
                switch (displayData)
                {
                    case ImageDisplayData imageDisplayData:
                        DBLogger.BLog.AddEntry("Found", $"{imageDisplayData.ToString()}");
                        imageDisplayData.texture = DBInitial.Kernel.DataStorage.GetTextureData(dataPackage.DataName, imageDisplayData.path);
                        if (!slot.Displays.MeshDisplayInitialized)
                        {
                            DBLogger.BLog.AddEntry("Pick From Pool", $"Unity Mesh Slot Display");
                            UnityEngineMeshSlotDisplay meshDisplay = unityMeshDisplaysPool.Pick();
   //                         imageDisplayData.texture = DBInitial.Kernel.DataStorage.GetTextureData(dataPackage.TextureAtlasName, imageDisplayData.path);
                            meshDisplay.Build(displayData, slot);
                            
                            slot.Displays.InitMeshDisplay(meshDisplay);
                        } break;
                    case MeshDisplayData meshDisplayData:
                        if (!slot.Displays.MeshDisplayInitialized)
                        {
                            DBLogger.BLog.AddEntry("Pick From Pool", $"Unity Mesh Slot Display");
                            UnityEngineMeshSlotDisplay meshDisplay = unityMeshDisplaysPool.Pick();
//                            meshDisplayData.texture = DBInitial.Kernel.DataStorage.GetTextureData(, meshDisplayData.path);
                            meshDisplay.Build(displayData, slot);
                            
                            slot.Displays.InitMeshDisplay(meshDisplay);
                        } break;
                    case ChildArmatureDisplayData childArmatureDisplayData:
                        DBLogger.BLog.AddEntry("Found", "Child Armature Display Data", childArmatureDisplayData.Name);
                        childArmatureDisplayData.armature = DBInitial.Kernel.DataStorage.GetArmatureData(childArmatureDisplayData.path, childArmatureDisplayData.BelongsToSkin.BelongsToArmature.parent.name);
                        UnityEngineChildArmatureSlotDisplay childArmature = BuildChildArmatureDisplay(dataPackage, slot, childArmatureDisplayData) as UnityEngineChildArmatureSlotDisplay;
                        childArmatureDisplayData.armature = childArmature.ArmatureDisplay.Armature.ArmatureData;

                        slot.Displays.AddChildArmatureDisplay(childArmature);
                        break;
                }
            }
            
            slot.Displays.SetAllDisplays(displayDatas.ToArray());
            
            slot.EndSlotBuilding();
            slot.EndUnitySlotBuilding();
            
            slot.Displays.SwapDisplaysByIndex(slotData.displayIndex);
            
            DBLogger.BLog.EndSection();
            return slot;
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

        public static void RefreshTextureAtlas(UnityTextureAtlasData textureAtlasData, bool isUGUI)
        {
            Material material = null;
            
            switch (isUGUI)
            {
                case true when textureAtlasData.uiTexture == null:
                {
                    material = Resources.Load<Material>(textureAtlasData.imagePath + "_UI_Mat");

                    if (material == null)
                    {
                        Texture2D textureAtlas = null;

                        textureAtlas = Resources.Load<Texture2D>(textureAtlasData.imagePath);

                        material = Helper.GenerateMaterial(defaultUIShaderName, textureAtlas.name + "_UI_Mat", textureAtlas);
                        if (textureAtlasData.width < 2)
                        {
                            textureAtlasData.width = (uint)textureAtlas.width;
                        }

                        if (textureAtlasData.height < 2)
                        {
                            textureAtlasData.height = (uint)textureAtlas.height;
                        }

                        textureAtlasData._disposeEnabled = true;
                    }

                    textureAtlasData.uiTexture = material;
                    break;
                }
                case false when textureAtlasData.texture == null:
                {
                    material = Resources.Load<Material>(textureAtlasData.imagePath + "_Mat");

                    if (material == null)
                    {
                        Texture2D textureAtlas = null;
                        textureAtlas = Resources.Load<Texture2D>(textureAtlasData.imagePath);

                        material = Helper.GenerateMaterial(defaultShaderName, textureAtlas.name + "_Mat", textureAtlas);
                        if (textureAtlasData.width < 2)
                        {
                            textureAtlasData.width = (uint)textureAtlas.width;
                        }

                        if (textureAtlasData.height < 2)
                        {
                            textureAtlasData.height = (uint)textureAtlas.height;
                        }

                        textureAtlasData._disposeEnabled = true;
                    }

                    textureAtlasData.texture = material;
                    break;
                }
            }
        }

        public void RefreshAllTextureAtlas(UnityEngineArmatureDisplay unityEngineArmature)
        {
            foreach (List<TextureAtlasData> textureAtlasDatas in DBInitial.Kernel.DataStorage.GetAllTextureAtlases().Values)
            {
                foreach (var atlas in textureAtlasDatas)
                {
                    var unityAtlas = (UnityTextureAtlasData)atlas;
                    RefreshTextureAtlas(unityAtlas, false);
                }
            }
        }

        public override void ReplaceDisplay(Slot slot, DisplayData displayData, int displayIndex = -1)
        {
            //UGUI Display Object and Normal Display Object cannot be replaced with each other
            if (displayData.type == DisplayType.Image || displayData.type == DisplayType.Mesh)
            {
                string dataName = displayData.BelongsToSkin.BelongsToArmature.parent.name;
                TextureData textureData = DBInitial.Kernel.DataStorage.GetTextureData(dataName, displayData.path);
                if (textureData != null)
                {
                    var textureAtlasData = textureData.parent as UnityTextureAtlasData;

                    var oldIsUGUI = false;

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
                    material = Helper.GenerateMaterial(defaultUIShaderName, texture.name + "_UI_Mat",
                        texture);
                }
                else
                {
                    material = Helper.GenerateMaterial(defaultShaderName, texture.name + "_Mat", texture);
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
                newDisplayData.Name = previousDisplayData.Name;
                newDisplayData.path = previousDisplayData.path;
                newDisplayData.DBTransform.CopyFrom(previousDisplayData.DBTransform);
                newDisplayData.BelongsToSkin = previousDisplayData.BelongsToSkin;
                (newDisplayData as ImageDisplayData).pivot.CopyFrom((previousDisplayData as ImageDisplayData).pivot);
                (newDisplayData as ImageDisplayData).texture = newTextureData;
            }
            else if (previousDisplayData is MeshDisplayData meshDisplayData)
            {
                newDisplayData = new MeshDisplayData();
                newDisplayData.type = previousDisplayData.type;
                newDisplayData.Name = previousDisplayData.Name;
                newDisplayData.path = previousDisplayData.path;
                newDisplayData.DBTransform.CopyFrom(previousDisplayData.DBTransform);
                newDisplayData.BelongsToSkin = previousDisplayData.BelongsToSkin;
                
                ((MeshDisplayData)newDisplayData).texture = newTextureData;
                ((MeshDisplayData)newDisplayData).vertices.inheritDeform = meshDisplayData.vertices.inheritDeform;
                ((MeshDisplayData)newDisplayData).vertices.offset = meshDisplayData.vertices.offset;
                ((MeshDisplayData)newDisplayData).vertices.data = meshDisplayData.vertices.data;
                ((MeshDisplayData)newDisplayData).vertices.weight = meshDisplayData.vertices.weight;
            }

            ReplaceDisplay(slot, newDisplayData, displayIndex);
        }

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