using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DragonBones
{
    public class Bone : TransformObject, IRegistryEntry
    {
        internal OffsetMode offsetMode;
        internal readonly DBTransform animationPose = new DBTransform();
        internal bool _transformDirty;
        internal bool _childrenTransformDirty;
        private bool _localDirty;
        internal bool _hasConstraint;
        private int _cachedFrameIndex;
        internal readonly BlendState _blendState = new BlendState();
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
            Visible = true;
            _cachedFrameIndex = -1;
            _blendState.Clear();
            boneData = null;
            ParentID = DBRegistry.EMPTY_ID;
            _cachedFrameIndices = null;
        }
        
        private void _UpdateGlobalTransformMatrix(bool isCache)
        {
            var boneData = this.boneData;
            var parent = this.parent;
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

                    if (this.boneData.inheritTranslation)
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

        internal void InitData(BoneData data)
        {
            boneData = data;
            if (boneData == null)
            {
                DBLogger.Error("Null BoneData provided to bone");
                return;
            }
            origin = boneData.DBTransform;
        }

        internal void BoneReady(DBRegistry.DBID armatureID, DBRegistry.DBID parentID)
        {
            ArmatureID = armatureID;
            ParentID = parentID;
        }

        public DBRegistry.DBID ParentID { get; protected set; }
        public DBRegistry.DBID ArmatureID { get; protected set; }

        internal void Update(int cacheFrameIndex, AnimationData currentStateAnimationData)
        {
            if (Armature == null) Armature = DB.Registry.GetArmature(ArmatureID);
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
                        foreach (DBRegistry.DBID id in DB.Registry.GetAllChildEntries<Constraint>(Armature.ID))
                        {
                            Constraint constraint = DB.Registry.GetEntry<Constraint>(id);
                            if (constraint._root == this)
                            {
                                constraint.Update();
                            }
                        }
                    }

                    if (_transformDirty || (parent != null && parent._childrenTransformDirty))
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
                    foreach (DBRegistry.DBID id in DB.Registry.GetAllChildEntries<Constraint>(Armature.ID))
                    {
                        Constraint constraint = DB.Registry.GetEntry<Constraint>(id);
                        if (constraint._root == this)
                        {
                            constraint.Update();
                        }
                    }
                }

                if (_transformDirty || (parent != null && parent._childrenTransformDirty))
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

                if (_transformDirty || (parent != null && parent._childrenTransformDirty))
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
        public BoneData boneData { get; internal set; }

        /// <summary>
        /// - The visible of all slots in the bone.
        /// </summary>
        /// <default>true</default>
        /// <see cref="DBKernel.Slot.visible"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public bool Visible { get; set; }

        /// <summary>
        /// - The bone name.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public string name => boneData.name;

        /// <summary>
        /// - The parent bone to which it belongs.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public Bone parent => DB.Registry.GetBone(ParentID);
        public DBRegistry.DBID ID { get; private set; }
        public void SetID(DBRegistry.DBID id) => ID = id;
    }
}
