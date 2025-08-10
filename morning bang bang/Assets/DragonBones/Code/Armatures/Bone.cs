using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    public class Bone : TransformObject
    {
        internal OffsetMode offsetMode;
        internal readonly DBTransform animationPose = new DBTransform();
        internal bool _transformDirty;
        internal bool _childrenTransformDirty;
        private bool _localDirty;
        internal bool _hasConstraint;
        private bool _visible;
        private int _cachedFrameIndex;
        internal readonly BlendState _blendState = new BlendState();
        internal BoneData _boneData;
        protected Bone _parent;
        internal List<int> _cachedFrameIndices = new List<int>();

        public override void OnReleased()
        {
            base.OnReleased();

            offsetMode = OffsetMode.Additive;
            animationPose.Identity();

            _transformDirty = false;
            _childrenTransformDirty = false;
            _localDirty = true;
            _hasConstraint = false;
            _visible = true;
            _cachedFrameIndex = -1;
            _blendState.Clear();
            _boneData = null; //
            _parent = null;
            _cachedFrameIndices = null;
        }
        /// <private/>
        private void _UpdateGlobalTransformMatrix(bool isCache)
        {
            var boneData = _boneData;
            var parent = _parent;
            var flipX = Armature.flipX;
            var flipY = Armature.flipY == DBKernel.IsNegativeYDown;
            var rotation = 0.0f;
            var global = this.global;
            var inherit = parent != null;
            var globalTransformMatrix = GlobalTransformDBMatrix;

            if (offsetMode == OffsetMode.Additive)
            {
                if (origin != null)
                {
                    //global.CopyFrom(this.origin).Add(this.offset).Add(this.animationPose);
                    global.x = origin.x + offset.x + animationPose.x;
                    global.y = origin.y + offset.y + animationPose.y;
                    global.skew = origin.skew + offset.skew + animationPose.skew;
                    global.rotation = origin.rotation + offset.rotation + animationPose.rotation;
                    global.scaleX = origin.scaleX * offset.scaleX * animationPose.scaleX;
                    global.scaleY = origin.scaleY * offset.scaleY * animationPose.scaleY;
                }
                else
                {
                    global.CopyFrom(offset).Add(animationPose);
                }

            }
            else if (offsetMode == OffsetMode.None)
            {
                if (origin != null)
                {
                    global.CopyFrom(origin).Add(animationPose);
                }
                else
                {
                    global.CopyFrom(animationPose);
                }
            }
            else
            {
                inherit = false;
                global.CopyFrom(offset);
            }

            if (inherit)
            {
                var parentMatrix = parent.GlobalTransformDBMatrix;
                if (boneData.inheritScale)
                {
                    if (!boneData.inheritRotation)
                    {
                        parent.UpdateGlobalTransform();

                        if (flipX && flipY)
                        {
                            rotation = global.rotation - (parent.global.rotation + DBTransform.PI);
                        }
                        else if (flipX)
                        {
                            rotation = global.rotation + parent.global.rotation + DBTransform.PI;
                        }
                        else if (flipY)
                        {
                            rotation = global.rotation + parent.global.rotation;
                        }
                        else
                        {
                            rotation = global.rotation - parent.global.rotation;
                        }

                        global.rotation = rotation;
                    }

                    global.ToMatrix(globalTransformMatrix);
                    globalTransformMatrix.Concat(parentMatrix);

                    if (_boneData.inheritTranslation)
                    {
                        global.x = globalTransformMatrix.tx;
                        global.y = globalTransformMatrix.ty;
                    }
                    else
                    {
                        globalTransformMatrix.tx = global.x;
                        globalTransformMatrix.ty = global.y;
                    }

                    if (isCache)
                    {
                        global.FromMatrix(globalTransformMatrix);
                    }
                    else
                    {
                        _globalDirty = true;
                    }
                }
                else
                {
                    if (boneData.inheritTranslation)
                    {
                        var x = global.x;
                        var y = global.y;
                        global.x = parentMatrix.a * x + parentMatrix.c * y + parentMatrix.tx;
                        global.y = parentMatrix.b * x + parentMatrix.d * y + parentMatrix.ty;
                    }
                    else
                    {
                        if (flipX)
                        {
                            global.x = -global.x;
                        }

                        if (flipY)
                        {
                            global.y = -global.y;
                        }
                    }

                    if (boneData.inheritRotation)
                    {
                        parent.UpdateGlobalTransform();
                        if (parent.global.scaleX < 0.0)
                        {
                            rotation = global.rotation + parent.global.rotation + DBTransform.PI;
                        }
                        else
                        {
                            rotation = global.rotation + parent.global.rotation;
                        }

                        if (parentMatrix.a * parentMatrix.d - parentMatrix.b * parentMatrix.c < 0.0)
                        {
                            rotation -= global.rotation * 2.0f;

                            if (flipX != flipY || boneData.inheritReflection)
                            {
                                global.skew += DBTransform.PI;
                            }
                        }

                        global.rotation = rotation;
                    }
                    else if (flipX || flipY)
                    {
                        if (flipX && flipY)
                        {
                            rotation = global.rotation + DBTransform.PI;
                        }
                        else
                        {
                            if (flipX)
                            {
                                rotation = DBTransform.PI - global.rotation;
                            }
                            else
                            {
                                rotation = -global.rotation;
                            }

                            global.skew += DBTransform.PI;
                        }

                        global.rotation = rotation;
                    }

                    global.ToMatrix(globalTransformMatrix);
                }
            }
            else
            {
                if (flipX || flipY)
                {
                    if (flipX)
                    {
                        global.x = -global.x;
                    }

                    if (flipY)
                    {
                        global.y = -global.y;
                    }

                    if (flipX && flipY)
                    {
                        rotation = global.rotation + DBTransform.PI;
                    }
                    else
                    {
                        if (flipX)
                        {
                            rotation = DBTransform.PI - global.rotation;
                        }
                        else
                        {
                            rotation = -global.rotation;
                        }

                        global.skew += DBTransform.PI;
                    }

                    global.rotation = rotation;
                }

                global.ToMatrix(globalTransformMatrix);
            }
        }
        /// <internal/>
        /// <private/>
        internal void Init(BoneData boneData, Armature armatureValue)
        {
            _boneData = boneData; 
            if (_boneData == null)
            {
                DBLogger.BLog.AddEntry("Bone Data Null", "Bone Init Aborted");
                return;
            }

            Armature = armatureValue;

            SetParentBone();

            origin = _boneData.DBTransform;
            DBLogger.BLog.AddEntry("Trying To Add Bone To Structure", boneData.name);
            Armature.Structure.AddBone(this);
        }

        private void SetParentBone()
        {
            if (_boneData.parent != null)
            {
                DBInitial.Kernel.Factory.CurLog().AddEntry("Set Parent Bone", name, $"{_boneData.parent.name}");
                _parent = Armature.Structure.GetBone(_boneData.parent.name);
            }
            DBLogger.BLog.AddEntry("Trying To Set Parent Bone", boneData.name, "No Parent Bone Found");
        }

        /// <internal/>
        /// <private/>
        internal void Update(int cacheFrameIndex, AnimationData currentStateAnimationData)
        {
            _blendState.dirty = false;

            if (cacheFrameIndex >= 0 && _cachedFrameIndices != null)
            {
                var cachedFrameIndex = _cachedFrameIndices[cacheFrameIndex];

                if (cachedFrameIndex >= 0 && _cachedFrameIndex == cachedFrameIndex)
                {
                    // Same cache.
                    _transformDirty = false;
                }
                else if (cachedFrameIndex >= 0)
                {
                    // Has been Cached.
                    _transformDirty = true;
                    _cachedFrameIndex = cachedFrameIndex;
                }
                else
                {
                    if (_hasConstraint)
                    {
                        // Update constraints.
                        foreach (var constraint in Armature.Structure.Constraints)
                        {
                            if (constraint._root == this)
                            {
                                constraint.Update();
                            }
                        }
                    }

                    if (_transformDirty || (_parent != null && _parent._childrenTransformDirty))
                    {
                        // Dirty.
                        _transformDirty = true;
                        _cachedFrameIndex = -1;
                    }
                    else if (_cachedFrameIndex >= 0)
                    {
                        // Same cache, but not set index yet.
                        _transformDirty = false;
                        _cachedFrameIndices[cacheFrameIndex] = _cachedFrameIndex;
                    }
                    else
                    {
                        // Dirty.
                        _transformDirty = true;
                        _cachedFrameIndex = -1;
                    }
                }
            }
            else
            {
                if (_hasConstraint)
                {
                    // Update constraints.
                    foreach (var constraint in Armature.Structure.Constraints)
                    {
                        if (constraint._root == this)
                        {
                            constraint.Update();
                        }
                    }
                }

                if (_transformDirty || (_parent != null && _parent._childrenTransformDirty))
                {
                    // Dirty.
                    cacheFrameIndex = -1;
                    _transformDirty = true;
                    _cachedFrameIndex = -1;
                }
            }

            if (_transformDirty)
            {
                _transformDirty = false;
                _childrenTransformDirty = true;

                var isCache = false;
                if (_localDirty)
                {
                    _UpdateGlobalTransformMatrix(isCache);
                }
            }
            else if (_childrenTransformDirty)
            {
                _childrenTransformDirty = false;
            }

            _localDirty = true;
        }
        /// <internal/>
        /// <private/>
        internal void UpdateByConstraint()
        {
            if (_localDirty)
            {
                _localDirty = false;

                if (_transformDirty || (_parent != null && _parent._childrenTransformDirty))
                {
                    _UpdateGlobalTransformMatrix(true);
                }

                _transformDirty = true;
            }
        }
        /// <summary>
        /// - Forces the bone to update the transform in the next frame.
        /// When the bone is not animated or its animation state is finished, the bone will not continue to update,
        /// and when the skeleton must be updated for some reason, the method needs to be called explicitly.
        /// </summary>
        /// <example>
        /// TypeScript style, for reference only.
        /// <pre>
        ///     let bone = armature.getBone("arm");
        ///     bone.offset.scaleX = 2.0;
        ///     bone.invalidUpdate();
        /// </pre>
        /// </example>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public void InvalidUpdate()
        {
            _transformDirty = true;
        }
        /// <summary>
        /// - Check whether the bone contains a specific bone or slot.
        /// </summary>
        /// <see cref="DBKernel.Bone"/>
        /// <see cref="DBKernel.Slot"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public bool Contains(Bone value)
        {
            if (value == this)
            {
                return false;
            }

            Bone ancestor = value;
            while (ancestor != this && ancestor != null)
            {
                ancestor = ancestor.parent;
            }

            return ancestor == this;
        }
        /// <summary>
        /// - The bone data.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public BoneData boneData
        {
            get { return _boneData; }
        }

        /// <summary>
        /// - The visible of all slots in the bone.
        /// </summary>
        /// <default>true</default>
        /// <see cref="DBKernel.Slot.visible"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public bool visible
        {
            get { return _visible; }
            set
            {
                if (_visible == value)
                {
                    return;
                }

                _visible = value;

                foreach (var slot in Armature.Structure.Slots)
                {
                    if (slot.Parent == this)
                    {
                        slot.Visible.Set(value);
                    }
                }
            }
        }

        /// <summary>
        /// - The bone name.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public string name
        {
            get { return _boneData.name; }
        }

        /// <summary>
        /// - The parent bone to which it belongs.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public Bone parent
        {
            get { return _parent; }
        }
    }
}
