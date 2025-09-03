using System.Collections.Generic;

namespace DragonBones
{
    /// <summary>
    /// - The animation config is used to describe all the information needed to play an animation state.
    /// The API is still in the experimental phase and may encounter bugs or stability or compatibility issues when used.
    /// </summary>
    /// <see cref="DBKernel.AnimationState"/>
    /// <beta/>
    /// <version>DragonBones 5.0</version>
    /// <language>en_US</language>
    public class AnimationConfig : DBObject
    {
        /// <private/>
        public bool pauseFadeOut;
        /// <summary>
        /// - Fade out the pattern of other animation states when the animation state is fade in.
        /// This property is typically used to specify the substitution of multiple animation states blend.
        /// </summary>
        /// <default>dragonBones.AnimationFadeOutMode.All</default>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public AnimationFadeOutMode fadeOutMode;
        /// <private/>
        public TweenType fadeOutTweenType;
        /// <private/>
        public float fadeOutTime;
        /// <private/>
        public bool pauseFadeIn;

        /// <private/>
        public bool actionEnabled;
        /// <private/>
        public bool additiveBlending;
        /// <summary>
        /// - Whether the animation state has control over the display property of the slots.
        /// Sometimes blend a animation state does not want it to control the display properties of the slots,
        /// especially if other animation state are controlling the display properties of the slots.
        /// </summary>
        /// <default>true</default>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public bool displayControl;
        /// <summary>
        /// - Whether to reset the objects without animation to the armature pose when the animation state is start to play.
        /// This property should usually be set to false when blend multiple animation states.
        /// </summary>
        /// <default>true</default>
        /// <version>DragonBones 5.1</version>
        /// <language>en_US</language>
        public bool resetToPose;
        /// <private/>
        public TweenType fadeInTweenType;
        /// <summary>
        /// - The play times. [0: Loop play, [1~N]: Play N times]
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public int playTimes;
        /// <summary>
        /// - The blend layer.
        /// High layer animation state will get the blend weight first.
        /// When the blend weight is assigned more than 1, the remaining animation states will no longer get the weight assigned.
        /// </summary>
        /// <readonly/>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public int layer;
        /// <summary>
        /// - The start time of play. (In seconds)
        /// </summary>
        /// <default>0.0</default>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public float position;
        /// <summary>
        /// - The duration of play.
        /// [-1: Use the default value of the animation data, 0: Stop play, (0~N]: The duration] (In seconds)
        /// </summary>
        /// <default>-1.0</default>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public float duration;
        /// <summary>
        /// - The play speed.
        /// The value is an overlay relationship with {@link dragonBones.Animation#timeScale}.
        /// [(-N~0): Reverse play, 0: Stop play, (0~1): Slow play, 1: Normal play, (1~N): Fast play]
        /// </summary>
        /// <default>1.0</default>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public float timeScale;
        /// <summary>
        /// - The blend weight.
        /// </summary>
        /// <default>1.0</default>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public float weight;
        /// <summary>
        /// - The fade in time.
        /// [-1: Use the default value of the animation data, [0~N]: The fade in time] (In seconds)
        /// </summary>
        /// <default>-1.0</default>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public float fadeInTime;
        /// <summary>
        /// - The auto fade out time when the animation state play completed.
        /// [-1: Do not fade out automatically, [0~N]: The fade out time] (In seconds)
        /// </summary>
        /// <default>-1.0</default>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public float autoFadeOutTime;
        /// <summary>
        /// - The name of the animation state. (Can be different from the name of the animation data)
        /// </summary>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public string name;
        /// <summary>
        /// - The animation data name.
        /// </summary>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public string animation;
        /// <summary>
        /// - The blend group name of the animation state.
        /// This property is typically used to specify the substitution of multiple animation states blend.
        /// </summary>
        /// <readonly/>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public string group;
        /// <private/>
        public readonly List<string> boneMask = new List<string>();

        /// <private/>
        public override void OnReleased()
        {
            this.pauseFadeOut = true;
            this.fadeOutMode = AnimationFadeOutMode.All;
            this.fadeOutTweenType = TweenType.Line;
            this.fadeOutTime = -1.0f;

            this.actionEnabled = true;
            this.additiveBlending = false;
            this.displayControl = true;
            this.pauseFadeIn = true;
            this.resetToPose = true;
            this.fadeInTweenType = TweenType.Line;
            this.playTimes = -1;
            this.layer = 0;
            this.position = 0.0f;
            this.duration = -1.0f;
            this.timeScale = -100.0f;
            this.weight = 1.0f;
            this.fadeInTime = -1.0f;
            this.autoFadeOutTime = -1.0f;
            this.name = "";
            this.animation = "";
            this.group = "";
            this.boneMask.Clear();
        }

        /// <private/>
        public void Clear()
        {
            this.OnReleased();
        }

        /// <private/>
        public void CopyFrom(AnimationConfig value)
        {
            this.pauseFadeOut = value.pauseFadeOut;
            this.fadeOutMode = value.fadeOutMode;
            this.autoFadeOutTime = value.autoFadeOutTime;
            this.fadeOutTweenType = value.fadeOutTweenType;

            this.actionEnabled = value.actionEnabled;
            this.additiveBlending = value.additiveBlending;
            this.displayControl = value.displayControl;
            this.pauseFadeIn = value.pauseFadeIn;
            this.resetToPose = value.resetToPose;
            this.playTimes = value.playTimes;
            this.layer = value.layer;
            this.position = value.position;
            this.duration = value.duration;
            this.timeScale = value.timeScale;
            this.fadeInTime = value.fadeInTime;
            this.fadeOutTime = value.fadeOutTime;
            this.fadeInTweenType = value.fadeInTweenType;
            this.weight = value.weight;
            this.name = value.name;
            this.animation = value.animation;
            this.group = value.group;

            boneMask.ResizeList(value.boneMask.Count, null);
            for (int i = 0, l = boneMask.Count; i < l; ++i)
            {
                boneMask[i] = value.boneMask[i];
            }
        }

        /// <private/>
        public bool ContainsBoneMask(string boneName)
        {
            return boneMask.Count == 0 || boneMask.Contains(boneName);
        }

        /// <private/>
        public void AddBoneMask(Armature armature, string boneName, bool recursive = false)
        {
            Bone currentBone = armature.Structure.GetBone(boneName);
            if (currentBone == null)
            {
                return;
            }

            if (!boneMask.Contains(boneName)) // Add mixing
            {
                boneMask.Add(boneName);
            }

            if (recursive) // Add recursive mixing.
            {
                for (int i = 0, l = armature.Structure.Bones.Length; i < l; ++i)
                {
                    Bone bone = armature.Structure.Bones[i];
                    
                    if (!boneMask.Contains(bone.Name) && currentBone.Contains(bone))
                    {
                        boneMask.Add(bone.Name);
                    }
                }
            }
        }

        /// <private/>
        public void RemoveBoneMask(Armature armature, string boneName, bool recursive = true)
        {
            if (boneMask.Contains(boneName)) // Remove mixing.
            {
                boneMask.Remove(boneName);
            }

            if (recursive)
            {
                Bone currentBone = armature.Structure.GetBone(boneName);
                if (currentBone != null)
                {
                    Bone[] bones = armature.Structure.Bones;
                    if (boneMask.Count > 0) // Remove recursive mixing.
                    {
                        for (int i = 0, l = bones.Length; i < l; ++i)
                        {
                            Bone bone = bones[i];
                            if (boneMask.Contains(bone.Name) && currentBone.Contains(bone))
                            {
                                boneMask.Remove(bone.Name);
                            }
                        }
                    }
                    else // Add unrecursive mixing.
                    {
                        for (int i = 0, l = bones.Length; i < l; ++i)
                        {
                            Bone bone = bones[i];
                            if (bone == currentBone)
                            {
                                continue;
                            }

                            if (!currentBone.Contains(bone))
                            {
                                boneMask.Add(bone.Name);
                            }
                        }
                    }
                }
            }
        }
    }
}