using System;
using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    public class BuildArmaturePackage
    {
        public string dataName = "";
        public string textureAtlasName = "";
        
        public DragonBonesData data;
        public ArmatureData armatureData;
        public SkinData skin;
        public object display;
    }
    
    /// <summary>
    /// - Base class for the factory that create the armatures. (Typically only one global factory instance is required)
    /// The factory instance create armatures by parsed and added DragonBonesData instances and TextureAtlasData instances.
    /// Once the data has been parsed, it has been cached in the factory instance and does not need to be parsed again until it is cleared by the factory instance.
    /// </summary>
    /// <see cref="DragonBonesData"/>
    /// ,
    /// <see cref="TextureAtlasData"/>
    /// ,
    /// <see cref="ArmatureData"/>
    /// ,
    /// <see cref="Armature"/>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public abstract class DBFactory
    {
        /// <summary>
        /// - Create a factory instance. (typically only one global factory instance is required)
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        /// <private/>
        protected bool _IsSupportMesh()
        {
            return true;
        }

        protected void BuildBonesFor(BuildArmaturePackage dataPackage, Armature armature)
        {
            var bones = dataPackage.armatureData.sortedBones;
            
            for (int i = 0, l = bones.Count; i < l; ++i)
            {
                var boneData = bones[i];
                var bone = DBObject.BorrowObject<Bone>();
                bone.Init(boneData, armature);
            }
        }
        
        protected void BuildSlotsFor(BuildArmaturePackage dataPackage, Armature armature)
        {
            var currentSkin = dataPackage.skin;
            var defaultSkin = dataPackage.armatureData.defaultSkin;
            
            if (currentSkin == null || defaultSkin == null)
            {
                return;
            }

            Dictionary<string, List<DisplayData>> skinSlots = new Dictionary<string, List<DisplayData>>();
            foreach (var key in defaultSkin.displays.Keys)
            {
                var displays = defaultSkin.GetDisplays(key);
                skinSlots[key] = displays;
            }

            if (currentSkin != defaultSkin)
            {
                foreach (var k in currentSkin.displays.Keys)
                {
                    var displays = currentSkin.GetDisplays(k);
                    skinSlots[k] = displays;
                }
            }

            foreach (var slotData in dataPackage.armatureData.sortedSlots)
            {
                List<DisplayData> displayDatas = skinSlots.ContainsKey(slotData.name) ? skinSlots[slotData.name] : null;
                Slot slot = BuildSlot(dataPackage, slotData, armature);
                slot.AllDisplaysData = displayDatas;

                if (displayDatas != null)
                {
                    var displayList = new List<object>();
                    for (int i = 0, l = displayDatas.Count; i < l; ++i)
                    {
                        var displayData = displayDatas[i];

                        if (displayData != null)
                        {
                            displayList.Add(GetSlotDisplay(dataPackage, displayData, null, slot));
                        }
                        else
                        {
                            displayList.Add(null);
                        }
                    }

                    slot._SetDisplayList(displayList);
                }

                slot._SetDisplayIndex(slotData.displayIndex, true);
            }
        }

        /// <private/>
        protected void BuildConstraintsFor(BuildArmaturePackage dataPackage, Armature armature)
        {
            var constraints = dataPackage.armatureData.constraints;
            foreach (var constraintData in constraints.Values)
            {
                // TODO more constraint type.
                var constraint = DBObject.BorrowObject<IKConstraint>();
                constraint.Init(constraintData, armature);
                armature.Structure.AddConstraint(constraint);
            }
        }

        protected virtual Armature BuildChildArmature(BuildArmaturePackage dataPackage, Slot slot, DisplayData displayData)
        {
            return BuildArmature(displayData.path, dataPackage != null ? dataPackage.dataName : "", "", dataPackage != null ? dataPackage.textureAtlasName : "");
        }
        
        protected object GetSlotDisplay(BuildArmaturePackage dataPackage, DisplayData displayData, DisplayData rawDisplayData, Slot slot)
        {
            var dataName = dataPackage != null ? dataPackage.dataName : displayData.parent.parent.parent.name;
            
            object display = null;
            
            switch (displayData.type)
            {
                case DisplayType.Image:
                    {
                        var imageDisplayData = displayData as ImageDisplayData;
                        
                        if (imageDisplayData.texture == null)
                        {
                            imageDisplayData.texture = DBInitial.Kernel.DataStorage.GetTextureData(dataName, displayData.path);
                        }
                        else if (dataPackage != null && !string.IsNullOrEmpty(dataPackage.textureAtlasName))
                        {
                            imageDisplayData.texture = DBInitial.Kernel.DataStorage.GetTextureData(dataPackage.textureAtlasName, displayData.path);
                        }

                        if (rawDisplayData != null && rawDisplayData.type == DisplayType.Mesh && _IsSupportMesh())
                        {
                            display = slot.MeshDisplay;
                        }
                        else
                        {
                            display = slot.RawDisplay;
                        }
                    }
                    break;
                case DisplayType.Mesh:
                    {
                        var meshDisplayData = displayData as MeshDisplayData;
                        if (meshDisplayData.texture == null)
                        {
                            meshDisplayData.texture = DBInitial.Kernel.DataStorage.GetTextureData(dataName, meshDisplayData.path);
                        }
                        else if (dataPackage != null && !string.IsNullOrEmpty(dataPackage.textureAtlasName))
                        {
                            meshDisplayData.texture = DBInitial.Kernel.DataStorage.GetTextureData(dataPackage.textureAtlasName, meshDisplayData.path);
                        }

                        if (_IsSupportMesh())
                        {
                            display = slot.MeshDisplay;
                        }
                        else
                        {
                            display = slot.RawDisplay;
                        }
                    } break;
                case DisplayType.Armature:
                {
                    ArmatureDisplayData armatureDisplayData = displayData as ArmatureDisplayData;
                    Armature childArmature = BuildChildArmature(dataPackage, slot, displayData);
                    
                    if (childArmature != null)
                    {
                        childArmature.inheritAnimation = armatureDisplayData.inheritAnimation;
                        if (!childArmature.inheritAnimation)
                        {
                            var actions = armatureDisplayData.actions.Count > 0 ? armatureDisplayData.actions : childArmature.ArmatureData.defaultActions;
                            if (actions.Count > 0)
                            {
                                foreach (var action in actions)
                                {
                                    var eventObject = DBObject.BorrowObject<EventObject>();
                                    EventObject.ActionDataToInstance(action, eventObject, slot.Armature);
                                    eventObject.slot = slot;
                                    slot.Armature.BufferAction(eventObject, false);
                                }
                            }
                            else
                            {
                                childArmature.AnimationPlayer.Play();
                            }
                        }

                        armatureDisplayData.armature = childArmature.ArmatureData; 
                    }

                    display = childArmature;
                } break;
                case DisplayType.BoundingBox:
                    break;
            }

            return display;
        }

        public abstract void Clear(bool disposeData = true);
        public abstract TextureAtlasData BuildTextureAtlasData(TextureAtlasData textureAtlasData, object textureAtlas);
        protected abstract Armature BuildArmatureProxy(BuildArmaturePackage dataPackage);
        protected abstract Slot BuildSlot(BuildArmaturePackage dataPackage, SlotData slotData, Armature armature);
       
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
        /// <see cref="DBKernel.DragonBonesData"/>
        /// <see cref="DBKernel.ArmatureData"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public virtual Armature BuildArmature(string armatureName, string dragonBonesName = "", string skinName = null, string textureAtlasName = null, IEngineArmatureDisplay display = null)
        {
            var dataPackage = new BuildArmaturePackage();
            
            if (!DBInitial.Kernel.DataStorage.FillBuildArmaturePackage(dataPackage, dragonBonesName, armatureName, skinName, textureAtlasName, display))
            {
                DBLogger.Assert(false, "No armature data: " + armatureName + ", " + (dragonBonesName != "" ? dragonBonesName : ""));
                return null;
            }

            Armature armature = BuildArmatureProxy(dataPackage);
            
            BuildBonesFor(dataPackage, armature);
            BuildSlotsFor(dataPackage, armature);
            BuildConstraintsFor(dataPackage, armature);
            
            armature.InvalidUpdate(null, true);
            armature.AdvanceTime(0.0f); // Update armature pose.

            return armature;
        }
        /// <private/>
        public virtual void ReplaceDisplay(Slot slot, DisplayData displayData, int displayIndex = -1)
        {
            if (displayIndex < 0)
            {
                displayIndex = slot.DisplayIndex;
            }

            if (displayIndex < 0)
            {
                displayIndex = 0;
            }

            slot.ReplaceDisplayData(displayData, displayIndex);

            var displayList = slot.DisplayList; // Copy.
            if (displayList.Count <= displayIndex)
            {
                displayList.ResizeList(displayIndex + 1);

                for (int i = 0, l = displayList.Count; i < l; ++i)
                {
                    // Clean undefined.
                    displayList[i] = null;
                }
            }

            if (displayData != null)
            {
                var rawDisplayDatas = slot.AllDisplaysData;
                DisplayData rawDisplayData = null;

                if (rawDisplayDatas != null)
                {
                    if (displayIndex < rawDisplayDatas.Count)
                    {
                        rawDisplayData = rawDisplayDatas[displayIndex];
                    }
                }

                displayList[displayIndex] = GetSlotDisplay(null, displayData, rawDisplayData, slot);
            }
            else
            {
                displayList[displayIndex] = null;
            }

            slot.DisplayList = displayList;
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
            var armatureData = DBInitial.Kernel.DataStorage.GetArmatureData(armatureName, dragonBonesName);
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
        /// <private/>
        public bool ReplaceSlotDisplayList(string dragonBonesName, string armatureName, string slotName, Slot slot)
        {
            var armatureData = DBInitial.Kernel.DataStorage.GetArmatureData(armatureName, dragonBonesName);
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
            // for (const displayData of displays) 
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
        /// <see cref="DBKernel.Armature"/>
        /// <see cref="DBKernel.SkinData"/>
        /// <version>DragonBones 5.6</version>
        /// <language>en_US</language>
        public bool ReplaceSkin(Armature armature, SkinData skin, bool isOverride = false, List<string> exclude = null)
        {
            var success = false;
            var defaultSkin = skin.parent.defaultSkin;

            foreach (var slot in armature.Structure.Slots)
            {
                if (exclude != null && exclude.Contains(slot.name))
                {
                    continue;
                }

                var displays = skin.GetDisplays(slot.name);
                if (displays == null)
                {
                    if (defaultSkin != null && skin != defaultSkin)
                    {
                        displays = defaultSkin.GetDisplays(slot.name);
                    }

                    if (displays == null)
                    {
                        if (isOverride)
                        {
                            slot.AllDisplaysData = null;
                            slot.DisplayList.Clear(); //
                        }

                        continue;
                    }
                }
                var displayCount = displays.Count;
                var displayList = slot.DisplayList; // Copy.
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
                slot.DisplayList = displayList;
            }

            return success;
        }
        /// <summary>
        /// - Replaces the existing animation data for a specific armature with the animation data for the specific armature data.
        /// This enables you to make a armature template so that other armature without animations can share it's animations.
        /// </summary>
        /// <param name="armature">- The armtaure.</param>
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
        /// <see cref="DBKernel.Armature"/>
        /// <see cref="DBKernel.ArmatureData"/>
        /// <version>DragonBones 5.6</version>
        /// <language>en_US</language>
        public bool ReplaceAnimation(Armature armature,
                                    ArmatureData armatureData,
                                    bool isOverride = true)
        {

            var skinData = armatureData.defaultSkin;
            if (skinData == null)
            {
                return false;
            }

            if (isOverride)
            {
                armature.AnimationPlayer.animations = armatureData.animations;
            }
            else
            {
                var rawAnimations = armature.AnimationPlayer.animations;
                Dictionary<string, AnimationData> animations = new Dictionary<string, AnimationData>();

                foreach (var k in rawAnimations.Keys)
                {
                    animations[k] = rawAnimations[k];
                }

                foreach (var k in armatureData.animations.Keys)
                {
                    animations[k] = armatureData.animations[k];
                }

                armature.AnimationPlayer.animations = animations;
            }

            foreach (var slot in armature.Structure.Slots)
            {
                var index = 0;
                foreach (var display in slot.DisplayList)
                {
                    if (display is Armature)
                    {
                        var displayDatas = skinData.GetDisplays(slot.name);
                        if (displayDatas != null && index < displayDatas.Count)
                        {
                            var displayData = displayDatas[index];
                            if (displayData != null && displayData.type == DisplayType.Armature)
                            {
                                var childArmatureData = DBInitial.Kernel.DataStorage.GetArmatureData(displayData.path, displayData.parent.parent.parent.name);

                                if (childArmatureData != null)
                                {
                                    ReplaceAnimation(display as Armature, childArmatureData, isOverride);
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// - An Worldclock instance updated by engine.
        /// </summary>
        /// <version>DragonBones 5.7</version>
        /// <language>en_US</language>
        public WorldClock clock
        {
            get { return DBInitial.Kernel.Clock; }
        }
        /// <summary>
        /// - Deprecated, please refer to {@link #replaceSkin}.
        /// </summary>
        /// <language>en_US</language>

        [Obsolete("")]
        public bool ChangeSkin(Armature armature, SkinData skin, List<string> exclude = null)
        {
            return ReplaceSkin(armature, skin, false, exclude);
        }
    }
    
}
