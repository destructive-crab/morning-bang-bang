using System.Collections.Generic;

namespace DragonBones
{
    /// <summary>
    /// - Armature is the core of the skeleton animation system.
    /// </summary> 
    /// <see cref="DragonBones.ArmatureStructure"/>, <see cref="DragonBones.ArmatureData"/>, <see cref="Bone"/>, <see cref="Slot"/>, <see cref="AnimationPlayer"/>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public class Armature : DBObject, IAnimatable
    {
        public readonly ArmatureStructure Structure;
        public readonly AnimationPlayer AnimationPlayer;
        
        public ArmatureData ArmatureData { get; set; }
        public IEngineArmatureDisplay Display { get; private set; } = null;
        public IEventDispatcher<EventObject> ArmatureEventDispatcher => Display;
        
        /// <summary>
        /// - The armature name.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public string Name => ArmatureData.name;
        
        /// <summary>
        /// - Whether to inherit the animation control of the parent armature.
        /// True to try to have the child armature play an animation with the same name when the parent armature play the animation.
        /// </summary>
        /// <default>true</default>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public bool inheritAnimation;

        public object userData;
        
        private bool _flipX;
        /// <summary>
        /// - Whether to flip the armature horizontally.
        /// </summary>
        /// <version>DragonBones 5.5</version>
        /// <language>en_US</language>
        public bool flipX
        {
            get { return _flipX; }
            set
            {
                if (_flipX == value)
                {
                    return;
                }

                _flipX = value;
                InvalidUpdate();
            }
        }
        
        private bool _flipY;
        /// <summary>
        /// - Whether to flip the armature vertically.
        /// </summary>
        /// <version>DragonBones 5.5</version>
        /// <language>en_US</language>
        public bool flipY
        {
            get { return _flipY; }
            set
            {
                if (_flipY == value)
                {
                    return;
                }

                _flipY = value;
                InvalidUpdate();
            }
        }
        
        internal int _cacheFrameIndex;
        /// <summary>
        /// - The animation cache frame rate, which turns on the animation cache when the set value is greater than 0.
        /// There is a certain amount of memory overhead to improve performance by caching animation data in memory.
        /// The frame rate should not be set too high, usually with the frame rate of the animation is similar and lower than the program running frame rate.
        /// When the animation cache is turned on, some features will fail, such as the offset property of bone.
        /// </summary>
        /// <example>
        /// TypeScript style, for reference only.
        /// <pre>
        ///     armature.cacheFrameRate = 24;
        /// </pre>
        /// </example>
        /// <see cref="DragonBonesData.frameRate"/>
        /// <see cref="ArmatureData.frameRate"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public uint cacheFrameRate
        {
            get { return ArmatureData.cacheFrameRate; }
            set
            {
                if (ArmatureData.cacheFrameRate != value)
                {
                    ArmatureData.CacheFrames(value);

                    // Set child armature frameRate.
                    foreach (var slot in Structure.Slots)
                    {
                        var childArmature = slot.ChildArmature;
                        if (childArmature != null)
                        {
                            childArmature.cacheFrameRate = value;
                        }
                    }
                }
            }
        }

        internal TextureAtlasData _replaceTextureAtlasData = null; // Initial value.
        private object _replacedTexture;
        /// <summary>
        /// - The display container.
        /// The display of the slot is displayed as the parent.
        /// Depending on the rendering engine, the type will be different, usually the DisplayObjectContainer type.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public object replacedTexture
        {
            get { return _replacedTexture; }
            set
            {
                if (_replacedTexture == value)
                {
                    return;
                }

                if (_replaceTextureAtlasData != null)
                {
                    _replaceTextureAtlasData.ReturnToPool();
                    _replaceTextureAtlasData = null;
                }

                _replacedTexture = value;

                foreach (var slot in Structure.Slots)
                {
                    slot.InvalidUpdate();
                    slot.Update(-1);
                }
            }
        }
        
        private WorldClock _clock = null; // Initial value.
        public WorldClock Clock
        {
            get => _clock;
            set
            {
                if (_clock == value) { return; }
                if (_clock != null) { _clock.Remove(this); }

                _clock = value;

                if (_clock != null) { _clock.Add(this); }

                // Update childArmature clock.
                foreach (Slot slot in Structure.Slots)
                {
                    Armature childArmature = slot.ChildArmature;
                    
                    if (childArmature != null)
                    {
                        childArmature.Clock = _clock;
                    }
                }
            }
        }
        
        /// <summary>
        /// - Get the parent slot which the armature belongs to.
        /// </summary>
        /// <see cref="Slot"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public Slot Parent { get; internal set; }
        
        private bool lockArmatureUpdate;
        private readonly List<EventObject> actions = new();

        public Armature()
        {
            Structure = new ArmatureStructure(this);
            AnimationPlayer = new AnimationPlayer(this);
        }
        
        internal void Initialize(ArmatureData armatureData, IEngineArmatureDisplay engineDisplay)
        {
            if (ArmatureData != null) return;

            ArmatureData = armatureData;
            Display = engineDisplay;

            Display.DBInit(this);
            AnimationPlayer.Init(ArmatureData.animations);
        }

        internal void ArmatureReady()
        {
            
        }

        /// <summary>
        /// - Dispose the armature. (Return to the object pool)
        /// </summary>
        /// <example>
        /// TypeScript style, for reference only.
        /// <pre>
        ///     removeChild(armature.display);
        ///     armature.dispose();
        /// </pre>
        /// </example>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public void Dispose()
        {
            if (ArmatureData != null)
            {
                lockArmatureUpdate = true;

                DBInitial.Kernel.BufferObject(this);
            }
        }

        protected override void ClearObject()
        {
            if (_clock != null)
            {
                // Remove clock first.
                _clock.Remove(this);
            }

            Display?.DBClear();
            _replaceTextureAtlasData?.ReturnToPool();

            inheritAnimation = true;
            userData = null;

            lockArmatureUpdate = false;
            _flipX = false;
            _flipY = false;
            _cacheFrameIndex = -1;

            Structure.Dispose();
            AnimationPlayer.Clear();
            actions.Clear();
            ArmatureData = null;
            Display = null;
            _replaceTextureAtlasData = null;
            _replacedTexture = null;
            _clock = null;
            Parent = null;
        }

        public void AdvanceTime(float passedTime)
        {
            //
            if (lockArmatureUpdate) return;
            //
            
            //asserts
            if (ArmatureData == null)
            {
                DBLogger.LogWarning("The armature has been disposed.");
                return;
            }

            if (ArmatureData.parent == null)
            {
                DBLogger.LogWarning("The armature data has been disposed.\nPlease make sure dispose armature before call factory.clear().");
                return;
            }
            
            int prevCacheFrameIndex = _cacheFrameIndex;

            // Update animation.
            AnimationPlayer.AdvanceTime(passedTime);
            
            if(Structure.SlotsDirty) Structure.SortSlotsListByZOrder();
            
            UpdateBonesAndSlots(_cacheFrameIndex, prevCacheFrameIndex);
            ProcessActions();

            Display.DBUpdate();
        }

        /// <summary>
        /// - Forces a specific bone or its owning slot to update the transform or display property in the next frame.
        /// </summary>
        /// <param name="boneName">- The bone name. (If not set, all bones will be update)</param>
        /// <param name="updateSlot">- Whether to update the bone's slots. (Default: false)</param>
        /// <see cref="Bone.InvalidUpdate()"/>
        /// <see cref="Slot.InvalidUpdate()"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public void InvalidUpdate(string boneName = null, bool updateSlot = false)
        {
            if (!string.IsNullOrEmpty(boneName))
            {
                Bone bone = Structure.GetBone(boneName);
                if (bone != null)
                {
                    bone.InvalidUpdate();

                    if (updateSlot)
                    {
                        foreach (var slot in Structure.Slots)
                        {
                            if (slot.Parent == bone)
                            {
                                slot.InvalidUpdate();
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var bone in Structure.Bones)
                {
                    bone.InvalidUpdate();
                }

                if (updateSlot)
                {
                    foreach (var slot in Structure.Slots)
                    {
                        slot.InvalidUpdate();
                    }
                }
            }
        }


        private void UpdateBonesAndSlots(int cacheFrameIndex, int prevCacheFrameIndex)
        {
            if (cacheFrameIndex < 0 || cacheFrameIndex != prevCacheFrameIndex)
            {
                int currentIndex;
                int indexGoal;

                for (currentIndex = 0, indexGoal = Structure.Bones.Length; currentIndex < indexGoal; ++currentIndex)
                {
                    Structure.Bones[currentIndex].Update(cacheFrameIndex);
                }

                for (currentIndex = 0, indexGoal = Structure.Slots.Length; currentIndex < indexGoal; ++currentIndex)
                {
                    Structure.Slots[currentIndex].Update(cacheFrameIndex);
                }
            }
        }

        private void ProcessActions()
        {
            if (actions.Count > 0)
            {
                lockArmatureUpdate = true;
                foreach (EventObject action in actions)
                {
                    ActionData actionData = action.actionData;
                    if (actionData != null)
                    {
                        if (actionData.type == ActionType.Play)
                        {
                            if (action.slot != null)
                            {
                                Armature childArmature = action.slot.ChildArmature;
                                if (childArmature != null)
                                {
                                    childArmature.AnimationPlayer.FadeIn(actionData.name);
                                }
                            }
                            else if (action.bone != null)
                            {
                                foreach (Slot slot in Structure.Slots)
                                {
                                    if (slot.Parent == action.bone)
                                    {
                                        Armature childArmature = slot.ChildArmature;
                                        if (childArmature != null)
                                        {
                                            childArmature.AnimationPlayer.FadeIn(actionData.name);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                AnimationPlayer.FadeIn(actionData.name);
                            }
                        }
                    }

                    action.ReturnToPool();
                }

                actions.Clear();
                lockArmatureUpdate = false;
            }
        }

        internal void BufferAction(EventObject action, bool append)
        {
            if (!actions.Contains(action))
            {
                if (append)
                {
                    actions.Add(action);
                }
                else
                {
                    actions.Insert(0, action);                    
                }
            }
        }
    }
}