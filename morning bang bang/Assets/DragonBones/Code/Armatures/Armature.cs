using System.Collections.Generic;

namespace DragonBones
{
    /// <summary>
    /// - Armature is the core of the skeleton animation system.
    /// </summary>
    /// <see cref="ArmatureData"/>
    ///,
    /// <see cref="Bone"/>
    ///,
    /// <see cref="Slot"/>
    ///,
    /// <see cref="AnimationPlayer"/>
    ///,
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public class Armature : DBObject, IAnimatable
    {
        /// <summary>
        /// - Whether to inherit the animation control of the parent armature.
        /// True to try to have the child armature play an animation with the same name when the parent armature play the animation.
        /// </summary>
        /// <default>true</default>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public bool inheritAnimation;
        public object userData;

        private bool _lockUpdate;
        private bool _slotsDirty;
        private bool _zOrderDirty;
        private bool _flipX;
        private bool _flipY;

        internal int _cacheFrameIndex;
        private readonly List<Bone> _bones = new();
        private readonly List<Slot> _slots = new();
        internal readonly List<Constraint> _constraints = new List<Constraint>();
        private readonly List<EventObject> _actions = new List<EventObject>();

        public ArmatureData _armatureData;
        private object _display;

        internal TextureAtlasData _replaceTextureAtlasData = null; // Initial value.
        private object _replacedTexture;

        private WorldClock _clock = null; // Initial value.

        internal Slot _parent;
        protected override void ClearObject()
        {
            if (_clock != null)
            {
                // Remove clock first.
                _clock.Remove(this);
            }

            foreach (var bone in _bones)
            {
                bone.ReturnToPool();
            }

            foreach (var slot in _slots)
            {
                slot.ReturnToPool();
            }

            foreach (var constraint in _constraints)
            {
                constraint.ReturnToPool();
            }

            if (AnimationPlayer != null)
            {
                AnimationPlayer.ReturnToPool();
            }

            if (Display != null)
            {
                Display.DBClear();
            }

            if (_replaceTextureAtlasData != null)
            {
                _replaceTextureAtlasData.ReturnToPool();
            }

            inheritAnimation = true;
            userData = null;

            _lockUpdate = false;
            _slotsDirty = false;
            _zOrderDirty = false;
            _flipX = false;
            _flipY = false;
            _cacheFrameIndex = -1;
            _bones.Clear();
            _slots.Clear();
            _constraints.Clear();
            _actions.Clear();
            _armatureData = null; 
            AnimationPlayer = null; 
            Display = null;
            _display = null;
            _replaceTextureAtlasData = null;
            _replacedTexture = null;
            _clock = null;
            _parent = null;
        }
        private static int OnSortSlots(Slot a, Slot b)
        {
            if (a._zOrder > b._zOrder)
            {
                return 1;
            }
            else if (a._zOrder < b._zOrder)
            {
                return -1;
            }

            return 0;//fixed slots sort error
        }
        internal void SortZOrder(short[] slotIndices, int offset)
        {
            var slotDatas = _armatureData.sortedSlots;
            var isOriginal = slotIndices == null;

            if (_zOrderDirty || !isOriginal)
            {
                for (int i = 0, l = slotDatas.Count; i < l; ++i)
                {
                    var slotIndex = isOriginal ? i : slotIndices[offset + i];
                    if (slotIndex < 0 || slotIndex >= l)
                    {
                        continue;
                    }

                    var slotData = slotDatas[slotIndex];
                    var slot = GetSlot(slotData.name);
                    if (slot != null)
                    {
                        slot._SetZOrder(i);
                    }
                }

                _slotsDirty = true;
                _zOrderDirty = !isOriginal;
            }
        }

        internal void AddBone(Bone value)
        {
            if (!_bones.Contains(value))
            {
                _bones.Add(value);
            }
        }

        internal void AddSlot(Slot value)
        {
            if (!_slots.Contains(value))
            {
                _slotsDirty = true;
                _slots.Add(value);
            }
        }


        internal void AddConstraint(Constraint value)
        {
            if (!_constraints.Contains(value))
            {
                _constraints.Add(value);
            }
        }

        internal void BufferAction(EventObject action, bool append)
        {
            if (!_actions.Contains(action))
            {
                if (append)
                {
                    _actions.Add(action);
                }
                else
                {
                    _actions.Insert(0, action);                    
                }
            }
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
            if (_armatureData != null)
            {
                _lockUpdate = true;

                DBInitial.Kernel.BufferObject(this);
            }
        }
        /// <internal/>
        /// <private/>
        internal void Init(ArmatureData armatureData, IEngineArmatureDisplay engineDisplay)
        {
            if (_armatureData != null)
            {
                return;
            }

            _armatureData = armatureData;
            AnimationPlayer = BorrowObject<AnimationPlayer>();
            Display = engineDisplay;

            Display.DBInit(this);
            AnimationPlayer.Init(this);
            AnimationPlayer.animations = _armatureData.animations;
        }
        /// <inheritDoc/>
        public void AdvanceTime(float passedTime)
        {
            if (_lockUpdate)
            {
                return;
            }

            if (_armatureData == null)
            {
                DBLogger.Assert(false, "The armature has been disposed.");
                return;
            }
            else if (_armatureData.parent == null)
            {
                DBLogger.Assert(false, "The armature data has been disposed.\nPlease make sure dispose armature before call factory.clear().");
                return;
            }

            var prevCacheFrameIndex = _cacheFrameIndex;

            // Update animation.
            AnimationPlayer.AdvanceTime(passedTime);
            if (this._slotsDirty)
            {
                this._slotsDirty = false;
                this._slots.Sort(OnSortSlots);
            }
            // Update bones and slots.
            if (_cacheFrameIndex < 0 || _cacheFrameIndex != prevCacheFrameIndex)
            {
                int i = 0, l = 0;
                for (i = 0, l = _bones.Count; i < l; ++i)
                {
                    _bones[i].Update(_cacheFrameIndex);
                }

                for (i = 0, l = _slots.Count; i < l; ++i)
                {
                    _slots[i].Update(_cacheFrameIndex);
                }
            }

            if (_actions.Count > 0)
            {
                _lockUpdate = true;
                foreach (var action in _actions)
                {
                    var actionData = action.actionData;
                    if (actionData != null)
                    {
                        if (actionData.type == ActionType.Play)
                        {
                            if (action.slot != null)
                            {
                                var childArmature = action.slot.ChildArmature;
                                if (childArmature != null)
                                {
                                    childArmature.AnimationPlayer.FadeIn(actionData.name);
                                }
                            }
                            else if (action.bone != null)
                            {
                                foreach (var slot in GetSlots())
                                {
                                    if (slot.parent == action.bone)
                                    {
                                        var childArmature = slot.ChildArmature;
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

                _actions.Clear();
                _lockUpdate = false;
            }

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
                Bone bone = GetBone(boneName);
                if (bone != null)
                {
                    bone.InvalidUpdate();

                    if (updateSlot)
                    {
                        foreach (var slot in _slots)
                        {
                            if (slot.parent == bone)
                            {
                                slot.InvalidUpdate();
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var bone in _bones)
                {
                    bone.InvalidUpdate();
                }

                if (updateSlot)
                {
                    foreach (var slot in _slots)
                    {
                        slot.InvalidUpdate();
                    }
                }
            }
        }
        /// <summary>
        /// - Check whether a specific point is inside a custom bounding box in a slot.
        /// The coordinate system of the point is the inner coordinate system of the armature.
        /// Custom bounding boxes need to be customized in Dragonbones Pro.
        /// </summary>
        /// <param name="x">- The horizontal coordinate of the point.</param>
        /// <param name="y">- The vertical coordinate of the point.</param>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public Slot ContainsPoint(float x, float y)
        {
            foreach (var slot in _slots)
            {
                if (slot.ContainsPoint(x, y))
                {
                    return slot;
                }
            }

            return null;
        }
        /// <summary>
        /// - Check whether a specific segment intersects a custom bounding box for a slot in the armature.
        /// The coordinate system of the segment and intersection is the inner coordinate system of the armature.
        /// Custom bounding boxes need to be customized in Dragonbones Pro.
        /// </summary>
        /// <param name="xA">- The horizontal coordinate of the beginning of the segment.</param>
        /// <param name="yA">- The vertical coordinate of the beginning of the segment.</param>
        /// <param name="xB">- The horizontal coordinate of the end point of the segment.</param>
        /// <param name="yB">- The vertical coordinate of the end point of the segment.</param>
        /// <param name="intersectionPointA">- The first intersection at which a line segment intersects the bounding box from the beginning to the end. (If not set, the intersection point will not calculated)</param>
        /// <param name="intersectionPointB">- The first intersection at which a line segment intersects the bounding box from the end to the beginning. (If not set, the intersection point will not calculated)</param>
        /// <param name="normalRadians">- The normal radians of the tangent of the intersection boundary box. [x: Normal radian of the first intersection tangent, y: Normal radian of the second intersection tangent] (If not set, the normal will not calculated)</param>
        /// <returns>The slot of the first custom bounding box where the segment intersects from the start point to the end point.</returns>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public Slot IntersectsSegment(float xA, float yA, float xB, float yB,
                                       Point intersectionPointA = null,
                                       Point intersectionPointB = null,
                                       Point normalRadians = null)
        {
            var isV = xA == xB;
            var dMin = 0.0f;
            var dMax = 0.0f;
            var intXA = 0.0f;
            var intYA = 0.0f;
            var intXB = 0.0f;
            var intYB = 0.0f;
            var intAN = 0.0f;
            var intBN = 0.0f;
            Slot intSlotA = null;
            Slot intSlotB = null;

            foreach (var slot in _slots)
            {
                var intersectionCount = slot.IntersectsSegment(xA, yA, xB, yB, intersectionPointA, intersectionPointB, normalRadians);
                if (intersectionCount > 0)
                {
                    if (intersectionPointA != null || intersectionPointB != null)
                    {
                        if (intersectionPointA != null)
                        {
                            var d = isV ? intersectionPointA.y - yA : intersectionPointA.x - xA;
                            if (d < 0.0f)
                            {
                                d = -d;
                            }

                            if (intSlotA == null || d < dMin)
                            {
                                dMin = d;
                                intXA = intersectionPointA.x;
                                intYA = intersectionPointA.y;
                                intSlotA = slot;

                                if (normalRadians != null)
                                {
                                    intAN = normalRadians.x;
                                }
                            }
                        }

                        if (intersectionPointB != null)
                        {
                            var d = intersectionPointB.x - xA;
                            if (d < 0.0f)
                            {
                                d = -d;
                            }

                            if (intSlotB == null || d > dMax)
                            {
                                dMax = d;
                                intXB = intersectionPointB.x;
                                intYB = intersectionPointB.y;
                                intSlotB = slot;

                                if (normalRadians != null)
                                {
                                    intBN = normalRadians.y;
                                }
                            }
                        }
                    }
                    else
                    {
                        intSlotA = slot;
                        break;
                    }
                }
            }

            if (intSlotA != null && intersectionPointA != null)
            {
                intersectionPointA.x = intXA;
                intersectionPointA.y = intYA;

                if (normalRadians != null)
                {
                    normalRadians.x = intAN;
                }
            }

            if (intSlotB != null && intersectionPointB != null)
            {
                intersectionPointB.x = intXB;
                intersectionPointB.y = intYB;

                if (normalRadians != null)
                {
                    normalRadians.y = intBN;
                }
            }

            return intSlotA;
        }
        /// <summary>
        /// - Get a specific bone.
        /// </summary>
        /// <param name="name">- The bone name.</param>
        /// <see cref="Bone"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public Bone GetBone(string name)
        {
            foreach (var bone in _bones)
            {
                if (bone.name == name)
                {
                    return bone;
                }
            }

            return null;
        }
        /// <summary>
        /// - Get a specific bone by the display.
        /// </summary>
        /// <param name="display">- The display object.</param>
        /// <see cref="Bone"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public Bone GetBoneByDisplay(object display)
        {
            var slot = GetSlotByDisplay(display);

            return slot != null ? slot.parent : null;
        }
        /// <summary>
        /// - Get a specific slot.
        /// </summary>
        /// <param name="name">- The slot name.</param>
        /// <see cref="Slot"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public Slot GetSlot(string name)
        {
            foreach (var slot in _slots)
            {
                if (slot.name == name)
                {
                    return slot;
                }
            }

            return null;
        }
        /// <summary>
        /// - Get a specific slot by the display.
        /// </summary>
        /// <param name="display">- The display object.</param>
        /// <see cref="Slot"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public Slot GetSlotByDisplay(object display)
        {
            if (display != null)
            {
                foreach (var slot in _slots)
                {
                    if (slot.Display == display)
                    {
                        return slot;
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// - Get all bones.
        /// </summary>
        /// <see cref="Bone"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public List<Bone> GetBones()
        {
            return _bones;
        }
        /// <summary>
        /// - Get all slots.
        /// </summary>
        /// <see cref="Slot"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public List<Slot> GetSlots()
        {
            return _slots;
        }

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
            get { return _armatureData.cacheFrameRate; }
            set
            {
                if (_armatureData.cacheFrameRate != value)
                {
                    _armatureData.CacheFrames(value);

                    // Set child armature frameRate.
                    foreach (var slot in _slots)
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
        /// <summary>
        /// - The armature name.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public string name
        {
            get { return _armatureData.name; }
        }
        /// <summary>
        /// - The armature data.
        /// </summary>
        /// <see cref="ArmatureData"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public ArmatureData armatureData
        {
            get { return _armatureData; }
        }
        /// <summary>
        /// - The animation player.
        /// </summary>
        /// <see cref="DBKernel.Animation"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public AnimationPlayer AnimationPlayer { get; private set; } = null;

        public IEngineArmatureDisplay Display { get; private set; } = null;

        /// <summary>
        /// - The EventDispatcher instance of the armature.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public IEventDispatcher<EventObject> eventDispatcher
        {
            get { return Display; }
        }
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

                foreach (var slot in _slots)
                {
                    slot.InvalidUpdate();
                    slot.Update(-1);
                }
            }
        }
        /// <inheritDoc/>
        public WorldClock clock
        {
            get { return _clock; }
            set
            {
                if (_clock == value)
                {
                    return;
                }

                if (_clock != null)
                {
                    _clock.Remove(this);
                }

                _clock = value;

                if (_clock != null)
                {
                    _clock.Add(this);
                }

                // Update childArmature clock.
                foreach (var slot in _slots)
                {
                    var childArmature = slot.ChildArmature;
                    if (childArmature != null)
                    {
                        childArmature.clock = _clock;
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
        public Slot parent
        {
            get { return _parent; }
        }

        public bool IsPivotApplied { get; private set; } = false;
        public int SlotsCount => _slots.Count;

        public void BonesBuildingFinished()
        {
            
        }
    }
}
