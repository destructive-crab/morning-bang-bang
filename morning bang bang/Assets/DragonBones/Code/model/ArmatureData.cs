using System.Collections.Generic;

namespace DragonBones
{
    /// <summary>
    /// - The armature data.
    /// </summary>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public class ArmatureData : DBObject
    {
        public ArmatureType type;

        /// <summary>
        /// - The animation frame rate.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        public uint frameRate;
        public uint cacheFrameRate;
        public float scale;
        /// <summary>
        /// - The armature name.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        public string name;
        public readonly Rectangle aabb = new Rectangle();
        /// <summary>
        /// - The names of all the animation data.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        public readonly List<string> animationNames = new List<string>();
        /// <private/>
        public readonly List<BoneData> sortedBones = new List<BoneData>();
        /// <private/>
        public readonly List<SlotData> sortedSlots = new List<SlotData>();
        /// <private/>
        public readonly List<ActionData> defaultActions = new List<ActionData>();
        /// <private/>
        public readonly List<ActionData> actions = new List<ActionData>();
        /// <private/>
        public readonly Dictionary<string, BoneData> bones = new Dictionary<string, BoneData>();
        /// <private/>
        public readonly Dictionary<string, SlotData> slots = new Dictionary<string, SlotData>();

        /// <private/>
        public readonly Dictionary<string, ConstraintData> constraints = new Dictionary<string, ConstraintData>();
        /// <private/>
        public readonly Dictionary<string, SkinData> skins = new Dictionary<string, SkinData>();
        /// <private/>
        public readonly Dictionary<string, AnimationData> animations = new Dictionary<string, AnimationData>();

        /// <summary>
        /// - The default skin data.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public SkinData defaultSkin = null;
        /// <summary>
        /// - The default animation data.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public AnimationData defaultAnimation = null;
        /// <private/>
        public CanvasData canvas = null; // Initial value.
        /// <private/>
        public UserData userData = null; // Initial value.
        /// <private/>
        public DBProjectData parent;
        /// <inheritDoc/>
        protected override void ClearObject()
        {
            foreach (var action in this.defaultActions)
            {
                action.ReturnToPool();
            }

            foreach (var action in this.actions)
            {
                action.ReturnToPool();
            }

            foreach (var k in this.bones.Keys)
            {
                this.bones[k].ReturnToPool();
            }

            foreach (var k in this.slots.Keys)
            {
                this.slots[k].ReturnToPool();
            }

            foreach (var k in this.constraints.Keys)
            {
                this.constraints[k].ReturnToPool();
            }

            foreach (var k in this.skins.Keys)
            {
                this.skins[k].ReturnToPool();
            }

            foreach (var k in this.animations.Keys)
            {
                this.animations[k].ReturnToPool();
            }

            if (this.canvas != null)
            {
                this.canvas.ReturnToPool();
            }

            if (this.userData != null)
            {
                this.userData.ReturnToPool();
            }

            this.type = ArmatureType.Armature;
            this.frameRate = 0;
            this.cacheFrameRate = 0;
            this.scale = 1.0f;
            this.name = "";
            this.aabb.Clear();
            this.animationNames.Clear();
            this.sortedBones.Clear();
            this.sortedSlots.Clear();
            this.defaultActions.Clear();
            this.actions.Clear();
            this.bones.Clear();
            this.slots.Clear();
            this.constraints.Clear();
            this.skins.Clear();
            this.animations.Clear();
            this.defaultSkin = null;
            this.defaultAnimation = null;
            this.canvas = null;
            this.userData = null;
            this.parent = null; //
        }

        public void SortBones()
        {
            var total = this.sortedBones.Count;
            if (total <= 0)
            {
                return;
            }

            var sortHelper = this.sortedBones.ToArray();
            var index = 0;
            var count = 0;
            this.sortedBones.Clear();
            while (count < total)
            {
                var bone = sortHelper[index++];
                if (index >= total)
                {
                    index = 0;
                }

                if (this.sortedBones.Contains(bone))
                {
                    continue;
                }

                var flag = false;
                foreach (var constraint in this.constraints.Values)
                {
                    // Wait constraint.
                    if (constraint.root == bone && !this.sortedBones.Contains(constraint.target))
                    {
                        flag = true;
                        break;
                    }
                }

                if (flag)
                {
                    continue;
                }
                if (bone.parent != null && !this.sortedBones.Contains(bone.parent))
                {
                    // Wait parent.
                    continue;
                }

                this.sortedBones.Add(bone);
                count++;
            }
        }

        public int SetCacheFrame(DBMatrix globalTransformDBMatrix, DBTransform dbTransform)
        {
            var dataArray = this.parent.cachedFrames;
            var arrayOffset = dataArray.Count;

            dataArray.ResizeList(arrayOffset + 10, 0.0f);

            dataArray[arrayOffset] = globalTransformDBMatrix.a;
            dataArray[arrayOffset + 1] = globalTransformDBMatrix.b;
            dataArray[arrayOffset + 2] = globalTransformDBMatrix.c;
            dataArray[arrayOffset + 3] = globalTransformDBMatrix.d;
            dataArray[arrayOffset + 4] = globalTransformDBMatrix.tx;
            dataArray[arrayOffset + 5] = globalTransformDBMatrix.ty;
            dataArray[arrayOffset + 6] = dbTransform.rotation;
            dataArray[arrayOffset + 7] = dbTransform.skew;
            dataArray[arrayOffset + 8] = dbTransform.scaleX;
            dataArray[arrayOffset + 9] = dbTransform.scaleY;

            return arrayOffset;
        }

        public void GetCacheFrame(DBMatrix globalTransformDBMatrix, DBTransform dbTransform, int arrayOffset)
        {
            var dataArray = this.parent.cachedFrames;
            globalTransformDBMatrix.a = dataArray[arrayOffset];
            globalTransformDBMatrix.b = dataArray[arrayOffset + 1];
            globalTransformDBMatrix.c = dataArray[arrayOffset + 2];
            globalTransformDBMatrix.d = dataArray[arrayOffset + 3];
            globalTransformDBMatrix.tx = dataArray[arrayOffset + 4];
            globalTransformDBMatrix.ty = dataArray[arrayOffset + 5];
            dbTransform.rotation = dataArray[arrayOffset + 6];
            dbTransform.skew = dataArray[arrayOffset + 7];
            dbTransform.scaleX = dataArray[arrayOffset + 8];
            dbTransform.scaleY = dataArray[arrayOffset + 9];
            dbTransform.x = globalTransformDBMatrix.tx;
            dbTransform.y = globalTransformDBMatrix.ty;
        }

        public void AddBone(BoneData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name))
            {
                if (this.bones.ContainsKey(value.name))
                {
                    DBLogger.Assert(false, "Same bone: " + value.name);
                    this.bones[value.name].ReturnToPool();
                }

                this.bones[value.name] = value;
                this.sortedBones.Add(value);
            }
        }

        public void AddSlot(SlotData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name))
            {
                if (this.slots.ContainsKey(value.name))
                {
                    DBLogger.Assert(false, "Same slot: " + value.name);
                    this.slots[value.name].ReturnToPool();
                }

                this.slots[value.name] = value;
                this.sortedSlots.Add(value);
            }
        }

        public void AddConstraint(ConstraintData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name))
            {
                if (this.constraints.ContainsKey(value.name))
                {
                    DBLogger.Assert(false, "Same constraint: " + value.name);
                    this.slots[value.name].ReturnToPool();
                }

                this.constraints[value.name] = value;
            }
        }

        public void AddSkin(SkinData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name))
            {
                if (this.skins.ContainsKey(value.name))
                {
                    DBLogger.Assert(false, "Same slot: " + value.name);
                    this.skins[value.name].ReturnToPool();
                }

                value.parent = this;
                this.skins[value.name] = value;
                if (this.defaultSkin == null)
                {
                    this.defaultSkin = value;
                }

                if (value.name == "default")
                {
                    this.defaultSkin = value;
                }
            }
        }

        public void AddAnimation(AnimationData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name))
            {
                if (this.animations.ContainsKey(value.name))
                {
                    DBLogger.Assert(false, "Same animation: " + value.name);
                    this.animations[value.name].ReturnToPool();
                }

                value.armatureData = this;
                this.animations[value.name] = value;
                this.animationNames.Add(value.name);
                if (this.defaultAnimation == null)
                {
                    this.defaultAnimation = value;
                }
            }
        }

        internal void AddAction(ActionData value, bool isDefault)
        {
            if (isDefault)
            {
                this.defaultActions.Add(value);
            }
            else
            {
                this.actions.Add(value);
            }
        }

        /// <summary>
        /// - Get a specific done data.
        /// </summary>
        /// <param name="boneName">- The bone name.</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public BoneData GetBone(string boneName)
        {
            return (!string.IsNullOrEmpty(boneName) && bones.ContainsKey(boneName)) ? bones[boneName] : null;
        }
        /// <summary>
        /// - Get a specific slot data.
        /// </summary>
        /// <param name="slotName">- The slot name.</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public SlotData GetSlot(string slotName)
        {
            return (!string.IsNullOrEmpty(slotName) && slots.ContainsKey(slotName)) ? slots[slotName] : null;
        }
        public ConstraintData GetConstraint(string constraintName)
        {
            return this.constraints.ContainsKey(constraintName) ? this.constraints[constraintName] : null;
        }
        /// <summary>
        /// - Get a specific skin data.
        /// </summary>
        /// <param name="skinName">- The skin name.</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public SkinData GetSkin(string skinName)
        {
            return !string.IsNullOrEmpty(skinName) ? (skins.ContainsKey(skinName) ? skins[skinName] : null) : defaultSkin;
        }

        public MeshDisplayData GetMesh(string skinName, string slotName, string meshName)
        {
            var skin = this.GetSkin(skinName);
            if (skin == null)
            {
                return null;
            }

            return skin.GetDisplay(slotName, meshName) as MeshDisplayData;
        }
        /// <summary>
        /// - Get a specific animation data.
        /// </summary>
        /// <param name="animationName">- The animation animationName.</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public AnimationData GetAnimation(string animationName)
        {
            return !string.IsNullOrEmpty(animationName) ? (animations.ContainsKey(animationName) ? animations[animationName] : null) : defaultAnimation;
        }
    }

    /// <summary>
    /// - The bone data.
    /// </summary>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public class BoneData : DBObject
    {
        /// <private/>
        public bool inheritTranslation;
        /// <private/>
        public bool inheritRotation;
        /// <private/>
        public bool inheritScale;
        /// <private/>
        public bool inheritReflection;
        /// <summary>
        /// - The bone length.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public float length;
        /// <summary>
        /// - The bone name.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public string name;
        /// <private/>
        public readonly DBTransform DBTransform = new DBTransform();
        /// <private/>
        public UserData userData = null; // Initial value.
        /// <summary>
        /// - The parent bone data.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public BoneData parent = null;

        /// <inheritDoc/>
        protected override void ClearObject()
        {
            if (this.userData != null)
            {
                this.userData.ReturnToPool();
            }

            this.inheritTranslation = false;
            this.inheritRotation = false;
            this.inheritScale = false;
            this.inheritReflection = false;
            this.length = 0.0f;
            this.name = "";
            this.DBTransform.Identity();
            this.userData = null;
            this.parent = null;
        }
    }

    /// <summary>
    /// - The slot data.
    /// </summary>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public class SlotData : DBObject
    {
        /// <internal/>
        /// <private/>
        public static readonly DBColor DefaultDBColor = new DBColor();

        /// <internal/>
        /// <private/>
        public static DBColor CreateColor()
        {
            return new DBColor();
        }

        /// <private/>
        public BlendMode blendMode;
        /// <private/>
        public int displayIndex;
        /// <private/>
        public int zOrder;
        /// <summary>
        /// - The slot name.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public string name;
        /// <private/>
        public DBColor DBColor = null; // Initial value.
        /// <private/>
        public UserData userData = null; // Initial value.
        /// <summary>
        /// - The parent bone data.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public BoneData parent;
        /// <inheritDoc/>
        protected override void ClearObject()
        {
            if (this.userData != null)
            {
                this.userData.ReturnToPool();
            }

            this.blendMode = BlendMode.Normal;
            this.displayIndex = 0;
            this.zOrder = 0;
            this.name = "";
            this.DBColor = null; //
            this.userData = null;
            this.parent = null; //
        }
    }
}