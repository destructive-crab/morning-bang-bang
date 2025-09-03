using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MothDIed.Pool;
using UnityEngine;

namespace DragonBones
{
    public class BuildArmaturePackage
    {
        public string DataName = "";
        public string TextureAtlasName = "";
        
        public DBProjectData DBProjectData;
        public ArmatureData ArmatureData;
        public SkinData Skin;

        public Armature Armature;
    }
    
    /// <summary>
    /// - Base class for the factory that create the armatures. (Typically only one global factory instance is required)
    /// The factory instance create armatures by parsed and added DragonBonesData instances and TextureAtlasData instances.
    /// Once the data has been parsed, it has been cached in the factory instance and does not need to be parsed again until it is cleared by the factory instance.
    /// </summary>
    /// <see cref="DBProjectData"/>, <see cref="TextureAtlasData"/>, <see cref="ArmatureData"/>, <see cref="Armature"/>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public class DBFactory
    {
        protected static bool IsSupportMesh() => true;

        /// <summary>
        /// - Create a armature from cached DragonBonesData instances and TextureAtlasData instances.
        /// Note that when the created armature that is no longer in use, you need to explicitly dispose {@link #dragonBones.Armature#dispose()}.
        /// </summary>
        /// <param name="armatureName">- The armature data name.</param>
        /// <param name="dragonBonesName">- The cached name of the DragonBonesData instance. (If not set, all DragonBonesData instances are retrieved, and when multiple DragonBonesData instances contain a the same name armature data, it may not be possible to accurately create a specific armature)</param>
        /// <param name="skinName">- The skin name, you can set a different ArmatureData name to share it's skin data. (If not set, use the default skin data)</param>
        /// <returns>The armature.</returns>
        /// <example>
        /// TypeScript style, for reference only.
        /// <pre>
        ///     let armature = factory.buildArmature("armatureName", "dragonBonesName");
        ///     armature.clock = factory.clock;
        /// </pre>
        /// </example>
        /// <see cref="DBProjectData"/>, <see cref="ArmatureData"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public virtual Armature BuildArmature(string armatureName, string dragonBonesName = "", string skinName = null, string textureAtlasName = null, IEngineArmatureRoot providedRoot = null)
        {
            Armature armature = GetEmptyArmature();
            IEngineArmatureRoot armatureRoot = null;

            if (providedRoot == null)
            {
                armatureRoot = GetNewArmatureDisplayFor(armature);
            }
            else
            {
                providedRoot.Armature?.ReleaseThis();
                providedRoot.DBClear();
                armatureRoot = providedRoot;
            }
            
            BuildArmaturePackage dataPackage = new BuildArmaturePackage();
            
            if (!DB.Kernel.DataStorage.FillBuildArmaturePackage(dataPackage, dragonBonesName, armatureName, skinName, textureAtlasName))
            {
                DBLogger.Warn("No armature data: " + armatureName + ", " + (dragonBonesName != "" ? dragonBonesName : ""));
                return null;
            }
            
            armature.Initialize(dataPackage.ArmatureData, armatureRoot);

            dataPackage.Armature = armature;
            armatureRoot.DBConnect(armature);
            
            BuildBonesFor(dataPackage, armature);
            BuildSlotsFor(dataPackage, armature);
            //todo
            BuildConstraintsFor(dataPackage, armature);
            
            armature.Structure.CompleteBuilding();
            
            DB.Kernel.Registry.Register(armature);

            foreach (Slot slot in armature.Structure.Slots)
            {
                slot.Display.Set(armature.Structure.GetDisplayByIndex(slot, slot.SlotData.DefaultDisplayIndex));
                slot.SlotReady();
            }
            
            armature.ArmatureReady();
            armature.Root.DBInit(armature);
            armature.InvalidUpdate(null, true);
            armature.AdvanceTime(0.0f); // Update armature pose.
            
            return armature;
        }

        protected void BuildBonesFor(BuildArmaturePackage dataPackage, Armature armature)
        {
            List<BoneData> bones = dataPackage.ArmatureData.sortedBones;
            
            for (int i = 0, l = bones.Count; i < l; ++i)
            {
                BoneData boneData = bones[i];
                Bone bone = DBObject.BorrowObject<Bone>();
                
                bone.ApplyData(boneData);
                bone.ApplyParentArmature(dataPackage.Armature);
                bone.ApplyParentBone(dataPackage.Armature.Structure.GetBone(boneData.parent.Name));
               
                bone.BoneReady();
            }
        }

        protected void BuildSlotsFor(BuildArmaturePackage dataPackage, Armature armature)
        {
            SkinData currentSkin = dataPackage.Skin;
            SkinData defaultSkin = dataPackage.ArmatureData.defaultSkin;
            
            if (currentSkin == null || defaultSkin == null) { return; }

            Dictionary<string, List<DisplayData>> skinSlots = new Dictionary<string, List<DisplayData>>();
            
            //strange dictionary creation method
            foreach (string key in defaultSkin.slotsAndTheirDisplays.Keys) { skinSlots.Add(key, defaultSkin.GetDisplays(key)); }

            if (currentSkin != defaultSkin)
            {
                foreach (var k in currentSkin.slotsAndTheirDisplays.Keys)
                {
                    List<DisplayData> displays = currentSkin.GetDisplays(k);
                    skinSlots[k] = displays;
                }
            }

            foreach (SlotData slotData in dataPackage.ArmatureData.sortedSlots)
            {
                List<DisplayData> displayDatas = skinSlots.ContainsKey(slotData.Name) ? skinSlots[slotData.Name] : null;
                BuildSlot(dataPackage, slotData, displayDatas, armature);
            }
        }

        protected void BuildConstraintsFor(BuildArmaturePackage dataPackage, Armature armature)
        {
            Dictionary<string, ConstraintData> constraints = dataPackage.ArmatureData.constraints;
            foreach (ConstraintData constraintData in constraints.Values)
            {
                // TODO more constraint type.
                IKConstraint constraint = DBObject.BorrowObject<IKConstraint>();

                Bone target = armature.Structure.GetBone(constraintData.target.Name);
                Bone bone = armature.Structure.GetBone(constraintData.bone.Name);
                Bone root = armature.Structure.GetBone(constraintData.root.Name);
                
                constraint.Init(constraintData, armature, target, root, bone);
                armature.Structure.RegisterConstraint(constraint);
            }
        }

        protected virtual Armature GetEmptyArmature()
        {
            return DBObject.BorrowObject<Armature>();
        }
        
        /// <summary>
        /// - Replaces the current display data for a particular slot with a specific display data.
        /// Specify display data with "dragonBonesName/armatureName/slotName/displayName".
        /// </summary>
        /// <param name="dragonBonesName">- The DragonBonesData instance cache name.</param>
        /// <param name="armatureName">- The armature data name.</param>
        /// <param name="slotName">- The slot data name.</param>
        /// <param name="displayName">- The display data name.</param>
        /// <param name="slot">- The slot.</param>
        /// <param name="displayIndex">- The index of the display data that is replaced. (If it is not set, replaces the current display data)</param>
        /// <example>
        /// TypeScript style, for reference only.
        /// <pre>
        ///     let slot = armature.getSlot("weapon");
        ///     factory.replaceSlotDisplay("dragonBonesName", "armatureName", "slotName", "displayName", slot);
        /// </pre>
        /// </example>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public bool ReplaceSlotDisplay(string dragonBonesName,
                                        string armatureName,
                                        string slotName,
                                        string displayName,
                                        Slot slot, int displayIndex = -1)
        {
            var armatureData = DB.Kernel.DataStorage.GetArmatureData(armatureName, dragonBonesName);
            if (armatureData == null || armatureData.defaultSkin == null)
            {
                return false;
            }

            var displayData = armatureData.defaultSkin.GetDisplay(slotName, displayName);
            if (displayData == null)
            {
                return false;
            }

            ReplaceDisplay(slot, displayData, displayIndex);

            return true;
        }
        public bool ReplaceSlotDisplayList(string dragonBonesName, string armatureName, string slotName, Slot slot)
        {
            var armatureData = DB.Kernel.DataStorage.GetArmatureData(armatureName, dragonBonesName);
            if (armatureData == null || armatureData.defaultSkin == null)
            {
                return false;
            }

            var displays = armatureData.defaultSkin.GetDisplays(slotName);
            if (displays == null)
            {
                return false;
            }

            var displayIndex = 0;
            for (int i = 0, l = displays.Count; i < l; ++i)
            {
                var displayData = displays[i];
                ReplaceDisplay(slot, displayData, displayIndex++);
            }

            return true;
        }
        /// <summary>
        /// - Share specific skin data with specific armature.
        /// </summary>
        /// <param name="armature">- The armature.</param>
        /// <param name="skin">- The skin data.</param>
        /// <param name="isOverride">- Whether it completely override the original skin. (Default: false)</param>
        /// <param name="exclude">- A list of slot names that do not need to be replace.</param>
        /// <example>
        /// TypeScript style, for reference only.
        /// <pre>
        ///     let armatureA = factory.buildArmature("armatureA", "dragonBonesA");
        ///     let armatureDataB = factory.getArmatureData("armatureB", "dragonBonesB");
        ///     if (armatureDataB && armatureDataB.defaultSkin) {
        ///     factory.replaceSkin(armatureA, armatureDataB.defaultSkin, false, ["arm_l", "weapon_l"]);
        ///     }
        /// </pre>
        /// </example>
        /// <see cref="Armature"/>
        /// <see cref="SkinData"/>
        /// <version>DragonBones 5.6</version>
        /// <language>en_US</language>
        public bool ReplaceSkin(Armature armature, SkinData skin, bool isOverride = false, List<string> exclude = null)
        {
           // var success = false;
           // var defaultSkin = skin.BelongsToArmature.defaultSkin;
//
           // foreach (var slot in armature.Structure.Slots)
           // {
           //     if (exclude != null && exclude.Contains(slot.Name))
           //     {
           //         continue;
           //     }
//
           //     var displays = skin.GetDisplays(slot.Name);
           //     if (displays == null)
           //     {
           //         if (defaultSkin != null && skin != defaultSkin)
           //         {
           //             displays = defaultSkin.GetDisplays(slot.Name);
           //         }
//
           //         if (displays == null)
           //         {
           //             if (isOverride)
           //             {
           //                 slot.Displays.Clear();
           //             }
//
           //             continue;
           //         }
           //     }
                
                /*int displayCount = displays.Count;
                List<object> displayList = slot.Displays.All; // Copy.
                displayList.ResizeList(displayCount); // Modify displayList length.
                
                for (int i = 0, l = displayCount; i < l; ++i)
                {
                    var displayData = displays[i];
                    if (displayData != null)
                    {
                        displayList[i] = GetSlotDisplay(null, displayData, null, slot);
                    }
                    else
                    {
                        displayList[i] = null;
                    }
                }

                success = true;
                slot.AllDisplaysData = displays;
                slot.DisplayList = displayList;*/
         //   }
//
         //   return success;
         return true;
        }
        /// <summary>
        /// - Replaces the existing animation data for a specific armature with the animation data for the specific armature data.
        /// This enables you to make a armature template so that other armature without animations can share it's animations.
        /// </summary>
        /// <param name="armature">- The armature.</param>
        /// <param name="armatureData">- The armature data.</param>
        /// <param name="isOverride">- Whether to completely overwrite the original animation. (Default: false)</param>
        /// <example>
        /// TypeScript style, for reference only.
        /// <pre>
        ///     let armatureA = factory.buildArmature("armatureA", "dragonBonesA");
        ///     let armatureDataB = factory.getArmatureData("armatureB", "dragonBonesB");
        ///     if (armatureDataB) {
        ///     factory.replaceAnimation(armatureA, armatureDataB);
        ///     }
        /// </pre>
        /// </example>
        /// <see cref="Armature"/>
        /// <see cref="ArmatureData"/>
        /// <version>DragonBones 5.6</version>
        /// <language>en_US</language>
        public bool ReplaceAnimation(Armature armature,
                                    ArmatureData armatureData,
                                    bool isOverride = true)
        {

         //   var skinData = armatureData.defaultSkin;
         //   if (skinData == null)
         //   {
         //       return false;
         //   }
//
         //   if (isOverride)
         //   {
         //       armature.AnimationPlayer.Animations = armatureData.animations;
         //   }
         //   else
         //   {
         //       Dictionary<string, AnimationData> rawAnimations = armature.AnimationPlayer.Animations;
         //       Dictionary<string, AnimationData> animations = new Dictionary<string, AnimationData>();
//
         //       foreach (var k in rawAnimations.Keys)
         //       {
         //           animations[k] = rawAnimations[k];
         //       }
//
         //       foreach (var k in armatureData.animations.Keys)
         //       {
         //           animations[k] = armatureData.animations[k];
         //       }
//
         //       armature.AnimationPlayer.Animations = animations;
         //   }
//
         //   foreach (Slot slot in armature.Structure.Slots)
         //   {
         //       int index = 0;
         //       //TODO
         //       //foreach (IEngineChildArmatureSlotDisplay display in slot.Displays.GetChildArmatureDisplays)
         //       //{
         //       //    var displayDatas = skinData.GetDisplays(slot.Name);
         //       //    if (displayDatas != null && index < displayDatas.Count)
         //       //    {
         //       //        var displayData = displayDatas[index];
////
         //       //        if (displayData != null && displayData.Type == DisplayType.Armature)
         //       //        {
         //       //            var childArmatureData = DBInitial.Kernel.DataStorage.GetArmatureData(displayData.path, displayData.BelongsToSkin.BelongsToArmature.parent.name);
////
         //       //            if (childArmatureData != null)
         //       //            {
         //       //                ReplaceAnimation(display.ArmatureDisplay.Armature, childArmatureData, isOverride);
         //       //            }
         //       //        }
         //       //    }
         //       //}
         //   }
//
            return true;
        }
        
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

        public void Clear(bool disposeData = true)
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

        protected IEngineChildArmature BuildChildArmatureDisplay(BuildArmaturePackage dataPackage, Slot forSlot, ChildArmatureDisplayData displayData, ChildArmatureDisplayData childArmatureData)
        {
            UnityChildArmature display = DBObject.BorrowObject<UnityChildArmature>();
            ChildArmature childArmature = DBObject.BorrowObject<ChildArmature>();

            BuildArmaturePackage childPackage = new();
            if (!DB.Kernel.DataStorage.FillBuildArmaturePackage(childPackage, childArmatureData.armature.belongsToProject.name, childArmatureData.armature.name, null, null))
            {
                DBLogger.Warn("No armature data: " + childArmatureData.armature.name);
                return null;
            }
            childArmature.InitializeChildArmature(displayData, forSlot);
            childArmature.Initialize(childArmatureData.armature, forSlot.ParentArmature.Root);
            
            BuildBonesFor(childPackage, childArmature);
            BuildSlotsFor(childPackage, childArmature);
            BuildConstraintsFor(childPackage, childArmature);
            
            childArmature.InvalidUpdate(null, true);
            childArmature.AdvanceTime(0.0f); // Update armature pose.
            
            childArmature.ArmatureReady();
            display.DBInit(childArmature, childArmatureData);
            
            return display;
        }

        protected IEngineArmatureRoot GetNewArmatureDisplayFor(Armature armature)
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

        private readonly List<ChildArmature> childArmaturesBuffer = new();
        protected Slot BuildSlot(BuildArmaturePackage dataPackage, SlotData slotData, List<DisplayData> displayDatas, Armature armature)
        {
            childArmaturesBuffer.Clear();
            UnitySlot slot = DBObject.BorrowObject<UnitySlot>();
            
            slot.StartBuilding(slotData, armature);
            slot.StarUnitySlotBuilding(armature.Root as UnityArmatureRoot);

            slot.ApplyParentBone(armature.Structure.GetBone(slotData.parent.Name));

            for (var i = 0; i < displayDatas.Count; i++)
            {
                DisplayData displayData = displayDatas[i];
                
                switch (displayData)
                {
                    case ImageDisplayData imageDisplayData:
                        imageDisplayData.texture = DB.Kernel.DataStorage.GetTextureData(dataPackage.DataName, imageDisplayData.path);
                        break;
                    case MeshDisplayData meshDisplayData:
                        meshDisplayData.texture = DB.Kernel.DataStorage.GetTextureData(dataPackage.DataName, meshDisplayData.path);
                        break;
                    case ChildArmatureDisplayData childArmatureDisplayData:
                        childArmatureDisplayData.armature = DB.Kernel.DataStorage.GetArmatureData(childArmatureDisplayData.path, childArmatureDisplayData.BelongsToSkin.BelongsToArmature.belongsToProject.name);
                        UnityChildArmature childArmature = BuildChildArmatureDisplay(dataPackage, slot, childArmatureDisplayData, childArmatureDisplayData) as UnityChildArmature;
                       
                        childArmaturesBuffer.Add(childArmature.Armature);
                        
                        childArmatureDisplayData.armature = childArmature.Armature.ArmatureData;
                        break;
                }
            }
            
            armature.Structure.RegisterSlot(slot);
            armature.Structure.RegisterDisplayData(slot, displayDatas.ToArray(), childArmaturesBuffer.ToArray());
            
            return slot;
        }

        public TextureAtlasData BuildTextureAtlasData(TextureAtlasData textureAtlasData, object textureAtlas)
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

                        material = UnityDBFactory.Helper.GenerateMaterial(defaultUIShaderName, textureAtlas.name + "_UI_Mat", textureAtlas);
                        
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

                        material = UnityDBFactory.Helper.GenerateMaterial(defaultShaderName, textureAtlas.name + "_Mat", textureAtlas);
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

        public void ReplaceDisplay(Slot slot, DisplayData displayData, int displayIndex = -1)
        {
            //UGUI Display Object and Normal Display Object cannot be replaced with each other
            if (displayData.Type == DisplayType.Image || displayData.Type == DisplayType.Mesh)
            {
                string dataName = displayData.BelongsToSkin.BelongsToArmature.belongsToProject.name;
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

            ReplaceDisplay(slot, displayData, displayIndex);
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
                    material = UnityDBFactory.Helper.GenerateMaterial(defaultUIShaderName, texture.name + "_UI_Mat",
                        texture);
                }
                else
                {
                    material = UnityDBFactory.Helper.GenerateMaterial(defaultShaderName, texture.name + "_Mat", texture);
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
    }
}
