using System.Collections.Generic;

namespace DragonBones
{
    public class ArmatureData : DBObject
    {
        public ArmatureType type;
        
        /// <summary> - The animation frame rate. </summary>
        public uint frameRate;
        public float scale;
        /// <summary> - The armature name. </summary>
        public string name;
        public readonly Rectangle aabb = new Rectangle();
        /// <summary> - The names of all the animation data. </summary>
        public readonly List<string> animationNames = new List<string>();
        public readonly List<BoneData> sortedBones = new List<BoneData>();
        public readonly List<SlotData> sortedSlots = new List<SlotData>();
        public readonly List<ActionData> defaultActions = new List<ActionData>();
        public readonly List<ActionData> actions = new List<ActionData>();
        
        public readonly Dictionary<string, BoneData> bones = new Dictionary<string, BoneData>();
        public readonly Dictionary<string, SlotData> slots = new Dictionary<string, SlotData>();

        public readonly Dictionary<string, ConstraintData> constraints = new Dictionary<string, ConstraintData>();
        public readonly Dictionary<string, SkinData> skins = new Dictionary<string, SkinData>();
        public readonly Dictionary<string, AnimationData> animations = new Dictionary<string, AnimationData>();

        /// <summary> - The default skin data. </summary>
        public SkinData defaultSkin = null;
        ///<summary> - The default animation data. </summary>
        public AnimationData defaultAnimation = null;
        
        public CanvasData canvas = null; // Initial value.
        public UserData userData = null; // Initial value.
        public DBProjectData belongsToProject;
        
        public override void OnReleased()
        {
            foreach (var action in defaultActions)
            {
                action.ReleaseThis();
            }

            foreach (var action in actions)
            {
                action.ReleaseThis();
            }

            foreach (string k in bones.Keys)
            {
                bones[k].ReleaseThis();
            }

            foreach (string k in slots.Keys)
            {
                slots[k].ReleaseThis();
            }

            foreach (string k in constraints.Keys)
            {
                constraints[k].ReleaseThis();
            }

            foreach (string k in skins.Keys)
            {
                skins[k].ReleaseThis();
            }

            foreach (string k in animations.Keys)
            {
                animations[k].ReleaseThis();
            }

            canvas?.ReleaseThis();
            userData?.ReleaseThis();

            type = ArmatureType.Armature;
            frameRate = 0;
            scale = 1.0f;
            name = "";
            aabb.Clear();
            animationNames.Clear();
            sortedBones.Clear();
            sortedSlots.Clear();
            defaultActions.Clear();
            actions.Clear();
            bones.Clear();
            slots.Clear();
            constraints.Clear();
            skins.Clear();
            animations.Clear();
            defaultSkin = null;
            defaultAnimation = null;
            canvas = null;
            userData = null;
            belongsToProject = null; 
        }

        public void SortBones()
        {
            int total = sortedBones.Count;
            if (total <= 0)
            {
                return;
            }

            BoneData[] sortHelper = sortedBones.ToArray();
            int index = 0;
            int count = 0;
            sortedBones.Clear();
            
            while (count < total)
            {
                BoneData bone = sortHelper[index++];
                if (index >= total)
                {
                    index = 0;
                }

                if (sortedBones.Contains(bone))
                {
                    continue;
                }

                bool flag = false;
                foreach (var constraint in constraints.Values)
                {
                    // Wait constraint.
                    if (constraint.root == bone && !sortedBones.Contains(constraint.target))
                    {
                        flag = true;
                        break;
                    }
                }

                if (flag)
                {
                    continue;
                }
                if (bone.parent != null && !sortedBones.Contains(bone.parent))
                {
                    // Wait parent.
                    continue;
                }

                sortedBones.Add(bone);
                count++;
            }
        }

        public void AddBone(BoneData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.Name))
            {
                if (bones.ContainsKey(value.Name))
                {
                    DBLogger.Assert(false, "Same bone: " + value.Name);
                    bones[value.Name].ReleaseThis();
                }

                bones[value.Name] = value;
                sortedBones.Add(value);
            }
        }

        public void AddSlot(SlotData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.Name))
            {
                if (slots.ContainsKey(value.Name))
                {
                    DBLogger.Assert(false, "Same slot: " + value.Name);
                    slots[value.Name].ReleaseThis();
                }

                slots[value.Name] = value;
                sortedSlots.Add(value);
            }
        }

        public void AddConstraint(ConstraintData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name))
            {
                if (constraints.ContainsKey(value.name))
                {
                    DBLogger.Error("Same constraint: " + value.name);
                    slots[value.name].ReleaseThis();
                }

                constraints[value.name] = value;
            }
        }

        public void AddSkin(SkinData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name))
            {
                if (skins.ContainsKey(value.name))
                {
                    DBLogger.Error("Same skin: " + value.name);
                    skins[value.name].ReleaseThis();
                }

                value.BelongsToArmature = this;
                skins[value.name] = value;
                if (defaultSkin == null)
                {
                    defaultSkin = value;
                }

                if (value.name == "default")
                {
                    defaultSkin = value;
                }
            }
        }

        public void AddAnimation(AnimationData value)
        {
            if (value != null && !string.IsNullOrEmpty(value.name))
            {
                if (animations.ContainsKey(value.name))
                {
                    DBLogger.Error("Same animation: " + value.name);
                    animations[value.name].ReleaseThis();
                }

                value.armatureData = this;
                animations[value.name] = value;
                animationNames.Add(value.name);
                if (defaultAnimation == null)
                {
                    defaultAnimation = value;
                }
            }
        }

        internal void AddAction(ActionData value, bool isDefault)
        {
            if (isDefault)
            {
                defaultActions.Add(value);
            }
            else
            {
                actions.Add(value);
            }
        }

        ///<summary> - Get a specific done data. </summary>
        public BoneData GetBone(string boneName)
        {
            return (!string.IsNullOrEmpty(boneName) && bones.ContainsKey(boneName)) ? bones[boneName] : null;
        }
        ///<summary> - Get a specific slot data. </summary>
        public SlotData GetSlot(string slotName)
        {
            return (!string.IsNullOrEmpty(slotName) && slots.ContainsKey(slotName)) ? slots[slotName] : null;
        }
        ///<summary> - Get a specific constraint data. </summary>
        public ConstraintData GetConstraint(string constraintName)
        {
            return constraints.ContainsKey(constraintName) ? constraints[constraintName] : null;
        }
        ///<summary> - Get a specific skin data. </summary>
        public SkinData GetSkin(string skinName)
        {
            return !string.IsNullOrEmpty(skinName) ? (skins.ContainsKey(skinName) ? skins[skinName] : null) : defaultSkin;
        }

        ///<summary> - Get a specific mesh display in specific skin. </summary>
        public MeshDisplayData GetMesh(string skinName, string slotName, string meshName)
        {
            SkinData skin = GetSkin(skinName);

            return skin?.GetDisplay(slotName, meshName) as MeshDisplayData;
        }
        ///<summary> - Get a specific animation data. </summary>
        public AnimationData GetAnimation(string animationName)
        {
            return !string.IsNullOrEmpty(animationName) ? (animations.ContainsKey(animationName) ? animations[animationName] : null) : defaultAnimation;
        }
    }

    public class BoneData : DBObject
    {
        public bool inheritTranslation;
        public bool inheritRotation;
        public bool inheritScale;
        public bool inheritReflection;
        
        ///<summary> - The bone length. </summary>
        public float length;
        ///<summary> - The bone name. </summary>
        public string Name;
        public readonly DBTransform DBTransform = new DBTransform();
        public UserData userData = null; // Initial value.
        ///<summary> - The parent bone data. </summary>
        public BoneData parent = null;

        public override void OnReleased()
        {
            if (userData != null)
            {
                userData.ReleaseThis();
            }

            inheritTranslation = false;
            inheritRotation = false;
            inheritScale = false;
            inheritReflection = false;
            length = 0.0f;
            Name = "";
            DBTransform.Identity();
            userData = null;
            parent = null;
        }
    }

    ///<summary> - The slot data. </summary>
    public class SlotData : DBObject
    {
        public static readonly DBColor DefaultDBColor = new DBColor();

        public static DBColor CreateColor()
        {
            return new DBColor();
        }

        public BlendMode blendMode;
        public int DefaultDisplayIndex;
        public int zOrder;
        ///<summary> - The slot name. </summary>
        public string Name;
        public DBColor DBColor = null; // Initial value.
        public UserData userData = null; // Initial value.
        ///<summary> - The parent bone data. </summary>
        public BoneData parent;
        public override void OnReleased()
        {
            if (userData != null)
            {
                userData.ReleaseThis();
            }

            blendMode = BlendMode.Normal;
            DefaultDisplayIndex = 0;
            zOrder = 0;
            Name = "";
            DBColor = null; 
            userData = null;
            parent = null; 
        }
    }
}