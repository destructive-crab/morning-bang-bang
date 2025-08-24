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
    public sealed class UnityDBFactory : DBFactory
    {
        internal const string defaultShaderName = "Sprites/Default";
        internal const string defaultUIShaderName = "UI/Default";
        
        private GameObjectPool<UnityArmatureRoot> unityArmatureDisplaysPool;

        public async UniTask InitializeFactory()
        {
            unityArmatureDisplaysPool = InitializeNewDBGOPool<UnityArmatureRoot>(64, "Armature Roots");

            await unityArmatureDisplaysPool.WarmAsync();
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
        
        public UnityArmatureRoot UnityCreateArmature(string armatureName, string dragonBonesName = "", UnityArmatureRoot root = null, string skinName = "", string textureAtlasName = "")
        {
            Armature armature = BuildArmature(armatureName, dragonBonesName, skinName, textureAtlasName, root);
            return armature.Root as UnityArmatureRoot;
        }

        protected override IEngineChildArmature BuildChildArmatureDisplay(BuildArmaturePackage dataPackage, Slot forSlot, DBRegistry.DBID displayID, ChildArmatureDisplayData childArmatureData)
        {
            UnityChildArmature display = DBObject.BorrowObject<UnityChildArmature>();
            ChildArmature childArmature = DBObject.BorrowObject<ChildArmature>();

            BuildArmaturePackage childPackage = new();
            if (!DB.Kernel.DataStorage.FillBuildArmaturePackage(childPackage, childArmatureData.armature.parent.name, childArmatureData.armature.name, null, null))
            {
                DBLogger.Warn("No armature data: " + childArmatureData.armature.name);
                return null;
            }

            childPackage.BuildID = dataPackage.BuildID;
            
            childArmature.InitializeChildArmature(forSlot);
            childArmature.Initialize(childArmatureData.armature, forSlot.Armature.Root);
            
            childPackage.ArmatureID = DB.Registry.RegisterChildArmature(dataPackage.BuildID, forSlot.ID, displayID, childArmature);
            
            BuildBonesFor(childPackage, childArmature);
            BuildSlotsFor(childPackage, childArmature);
            BuildConstraintsFor(childPackage, childArmature);
            
            childArmature.InvalidUpdate(null, true);
            childArmature.AdvanceTime(0.0f); // Update armature pose.
            
            childArmature.ArmatureReady();
            display.DBInit(childArmature, childArmatureData);
            
            return display;
        }

        protected override IEngineArmatureRoot GetNewArmatureDisplayFor(Armature armature)
        {
            if (armature == null)
            {
                DBLogger.Warn($"No armature instance was provided for building display");
                return null;
            }
            if (armature.Root != null)
            {
                DBLogger.Warn($"Armature ({armature.Name}) already has display");
                return armature.Root;
            }
            
            return unityArmatureDisplaysPool.Pick();
        }

        protected override Slot BuildSlot(BuildArmaturePackage dataPackage, SlotData slotData, List<DisplayData> displayDatas, Armature armature)
        {
            UnitySlot slot = DBObject.BorrowObject<UnitySlot>();
            
            slot.StartBuilding(slotData, armature);
            slot.StarUnitySlotBuilding(armature.Root as UnityArmatureRoot);

            DB.Kernel.Registry.RegisterSlot(dataPackage.BuildID,DB.Kernel.Registry.SearchInBuild(dataPackage.BuildID, slotData.parent.name, DBRegistry.BONE), slot);
            
            for (var i = 0; i < displayDatas.Count; i++)
            {
                DisplayData displayData = displayDatas[i];
                DBRegistry.DBID displayID = DB.Kernel.Registry.RegisterDisplay(dataPackage.BuildID, slot.ID, displayData, i);
                
                switch (displayData)
                {
                    case ImageDisplayData imageDisplayData:
                        imageDisplayData.texture = DB.Kernel.DataStorage.GetTextureData(dataPackage.DataName, imageDisplayData.path);
                        break;
                    case MeshDisplayData meshDisplayData:
                        meshDisplayData.texture = DB.Kernel.DataStorage.GetTextureData(dataPackage.DataName, meshDisplayData.path);
                        break;
                    case ChildArmatureDisplayData childArmatureDisplayData:
                        childArmatureDisplayData.armature = DB.Kernel.DataStorage.GetArmatureData(childArmatureDisplayData.path, childArmatureDisplayData.BelongsToSkin.BelongsToArmature.parent.name);
                        UnityChildArmature childArmature = BuildChildArmatureDisplay(dataPackage, slot, displayID, childArmatureDisplayData) as UnityChildArmature;
                        childArmatureDisplayData.armature = childArmature.Armature.ArmatureData;
                        break;
                }

            }
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

        public void RefreshAllTextureAtlas(UnityArmatureRoot unityArmature)
        {
            foreach (List<TextureAtlasData> textureAtlasDatas in DB.Kernel.DataStorage.GetAllTextureAtlases().Values)
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
            if (displayData.Type == DisplayType.Image || displayData.Type == DisplayType.Mesh)
            {
                string dataName = displayData.BelongsToSkin.BelongsToArmature.parent.name;
                TextureData textureData = DB.Kernel.DataStorage.GetTextureData(dataName, displayData.path);
                if (textureData != null)
                {
                    var textureAtlasData = textureData.parent as UnityTextureAtlasData;

                    var oldIsUGUI = false;

                    if ((oldIsUGUI && textureAtlasData.uiTexture == null) ||
                        (!oldIsUGUI && textureAtlasData.texture == null))
                    {
                        DBLogger.Warn(
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
            ArmatureData armatureData = DB.Kernel.DataStorage.GetArmatureData(armatureName, dragonBonesName);
            
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
                newDisplayData.Type = previousDisplayData.Type;
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
                newDisplayData.Type = previousDisplayData.Type;
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

        private static readonly Queue<Mesh> meshPool = new();

        public static void ReleaseMesh(Mesh mesh)
        {
            if (mesh == null) return;
            
            mesh.Clear();
            meshPool.Enqueue(mesh);
        }
            
        public static Mesh GetEmptyMesh()
        {
            if (meshPool.TryPeek(out Mesh mesh)) return mesh;
                
            Mesh newMesh = new Mesh();
                
            newMesh.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            newMesh.MarkDynamic();

            return newMesh;
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