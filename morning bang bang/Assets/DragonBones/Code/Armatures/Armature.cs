using System.Collections.Generic;

namespace DragonBones
{
    /// <summary>
    /// - Armature is the core of the skeleton animation system.
    /// </summary> 
    /// <see cref="DragonBones.ArmatureStructure"/>, <see cref="DragonBones.ArmatureData"/>, <see cref="Bone"/>, <see cref="Slot"/>, <see cref="AnimationPlayer"/>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public class Armature : DBObject, IAnimatable, IRegistryEntry
    {
        public readonly AnimationPlayer AnimationPlayer;
        
        public ArmatureData ArmatureData { get; set; }
        public IEngineArmatureRoot Root { get; private set; } = null;
        public IEventDispatcher<EventObject> ArmatureEventDispatcher => Root;
        
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
                    _replaceTextureAtlasData.ReleaseThis();
                    _replaceTextureAtlasData = null;
                }

                _replacedTexture = value;

                //todo
        //        foreach (var slot in DBI.Registry.GetChildSlotsOf(ID))
        //        {
        //            slot.InvalidUpdate();
        //            slot.ProcessDirtyDisplay();
        //        }
            }
        }
        
        private bool lockArmatureUpdate;
        private readonly List<EventObject> actions = new();

        public Armature()
        {
            AnimationPlayer = new AnimationPlayer(this);
        }
        
        internal virtual void Initialize(ArmatureData armatureData, IEngineArmatureRoot engineRoot)
        {
            ArmatureData = armatureData;
            Root = engineRoot;

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

                DB.Kernel.BufferObject(this);
            }
        }

        public override void OnReleased()
        {
            Root?.DBClear();
            _replaceTextureAtlasData?.ReleaseThis();

            inheritAnimation = true;
            userData = null;

            lockArmatureUpdate = false;
            _flipX = false;
            _flipY = false;

            AnimationPlayer.Clear();
            actions.Clear();
            ArmatureData = null;
            Root = null;
            _replaceTextureAtlasData = null;
            _replacedTexture = null;
        }

        public void AdvanceTime(float passedTime)
        {
            //
            if (lockArmatureUpdate) return;
            //
            
            //asserts
            if (ArmatureData == null)
            {
                DBLogger.Warn("The armature has been disposed.");
                return;
            }

            if (ArmatureData.parent == null)
            {
                DBLogger.Warn("The armature data has been disposed.\nPlease make sure dispose armature before call factory.clear().");
                return;
            }
            
            // Update animation.
            AnimationPlayer.AdvanceTime(passedTime);
            UpdateBonesAndSlots();
            
            if(Root.Armature == this) Root.DBUpdate();
            ProcessActions();
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
            foreach (DBRegistry.DBID id in DB.Registry.GetBones(ID))
            {
                DB.Registry.GetBone(id).InvalidUpdate();
            }

            if (updateSlot)
            {
                foreach (DBRegistry.DBID id in DB.Registry.GetChildSlotsOf(ID))
                {
                    DB.Registry.GetSlot(id).InvalidUpdate();
                }
            }
        }

        private DBRegistry.DBID[] cachedSlots = null;
        private DBRegistry.DBID[] cachedBones = null;
        private void UpdateBonesAndSlots()
        {
            if (cachedBones == null || cachedSlots == null)
            {
                DBRegistry.DBID[] ids = DB.Registry.GetAllChildEntries<Bone>(ID);
                cachedBones = new DBRegistry.DBID[ids.Length];
                for (int i = 0; i < ids.Length; i++)
                {
                    cachedBones[i] = ids[i];
                }
                
                ids = DB.Registry.GetAllChildEntries<Slot>(ID);
                cachedSlots = new DBRegistry.DBID[ids.Length];
                for (int i = 0; i < ids.Length; i++)
                {
                    cachedSlots[i] = ids[i];
                }               
            }
            
            foreach (DBRegistry.DBID id in cachedBones)
            {
                DB.Registry.GetBone(id).Update(-1, null);
            }
            
            foreach (DBRegistry.DBID id in cachedSlots)
            {
                Slot slot = DB.Registry.GetSlot(id);
               
                slot.ProcessDirtyDisplay();
                if (CachingEnabled && AnimationPlayer.CurrentState != null)
                {
                    slot.UpdateCache(AnimationPlayer.CurrentState.Animation, DB.Kernel.Cacher, AnimationPlayer.CurrentState.CurrentCacheFrameIndex);
                }
                slot.ProcessDirtyData();
            }
        }

        public bool CachingEnabled { get; set; } = false;
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
                                if (action.slot.IsDisplayingChildArmature())
                                {
                                    DB.Registry.GetChildArmatureFromDisplayID(action.slot.DisplayID.V).AnimationPlayer.FadeIn(actionData.name);
                                }
                            }
                            else if (action.bone != null)
                            {
                                foreach (DBRegistry.DBID id in DB.Registry.GetAllChildArmaturesOf(action.bone.ID))
                                {
                                    DB.Registry.GetChildArmature(id).AnimationPlayer.FadeIn(actionData.name);
                                }
                            }
                            else
                            {
                                AnimationPlayer.FadeIn(actionData.name);
                            }
                        }
                    }

                    action.ReleaseThis();
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

        public DBRegistry.DBID ID { get; private set; } = DBRegistry.EMPTY_ID;

        public void SetID(DBRegistry.DBID id)
        {
            ID = id;
        }
    }
}