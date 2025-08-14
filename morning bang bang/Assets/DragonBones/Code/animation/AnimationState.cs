using System;
using System.Collections.Generic;

namespace DragonBones
{
    /// <summary>
    /// - The animation state is generated when the animation data is played.
    /// </summary>
    /// <see cref="DBKernel.Animation"/>
    /// <see cref="DBKernel.AnimationData"/>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public class AnimationState : DBObject
    {
        /// <private/>
        public bool actionEnabled;
        /// <private/>
        public bool additiveBlending;
        /// <summary>
        /// - Whether the animation state has control over the display object properties of the slots.
        /// Sometimes blend a animation state does not want it to control the display object properties of the slots,
        /// especially if other animation state are controlling the display object properties of the slots.
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
        /// - The auto fade out time when the animation state play completed.
        /// [-1: Do not fade out automatically, [0~N]: The fade out time] (In seconds)
        /// </summary>
        /// <default>-1.0</default>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public float autoFadeOutTime;
        /// <private/>
        public float fadeTotalTime;
        /// <summary>
        /// - The name of the animation state. (Can be different from the name of the animation data)
        /// </summary>
        /// <readonly/>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public string name;
        /// <summary>
        /// - The blend group name of the animation state.
        /// This property is typically used to specify the substitution of multiple animation states blend.
        /// </summary>
        /// <readonly/>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public string group;

        private int _timelineDirty;
        /// <summary>
        /// - xx: Play Enabled, Fade Play Enabled
        /// </summary>
        /// <internal/>
        /// <private/>
        internal int _playheadState;
        /// <summary>
        /// -1: Fade in, 0: Fade complete, 1: Fade out;
        /// </summary>
        /// <internal/>
        /// <private/>
        internal int _fadeState;
        /// <summary>
        /// -1: Fade start, 0: Fading, 1: Fade complete;
        /// </summary>
        /// <internal/>
        /// <private/>
        internal int _subFadeState;
        /// <internal/>
        /// <private/>
        internal float _position;
        /// <internal/>
        /// <private/>
        internal float _duration;
        private float _fadeTime;
        private float _time;
        /// <internal/>
        /// <private/>
        internal float _fadeProgress;
        /// <internal/>
        /// <private/>
        private float _weightResult;
        /// <internal/>
        /// <private/>
        internal readonly BlendState _blendState = new BlendState();
        private readonly List<string> _boneMask = new List<string>();
        private readonly List<BoneTimelineState> _boneTimelines = new List<BoneTimelineState>();
        private readonly List<SlotTimelineState> _slotTimelines = new List<SlotTimelineState>();
        private readonly List<ConstraintTimelineState> _constraintTimelines = new List<ConstraintTimelineState>();
        private readonly List<TimelineState> _poseTimelines = new List<TimelineState>();
        private readonly Dictionary<string, BonePose> _bonePoses = new Dictionary<string, BonePose>();
        /// <internal/>
        /// <private/>
        public AnimationData Animation;
        private Armature _armature;
        /// <internal/>
        /// <private/>
        internal ActionTimelineState _actionTimeline = null; // Initial value.
        private ZOrderTimelineState _zOrderTimeline = null; // Initial value.
        /// <internal/>
        /// <private/>
        public AnimationState _parent = null; // Initial value.

        public int CurrentCacheFrameIndex { get; private set; }
        private int previousCacheFrameIndex;

        public override void OnReleased()
        {
            foreach (var timeline in _boneTimelines)
            {
                timeline.ReleaseThis();
            }

            foreach (var timeline in _slotTimelines)
            {
                timeline.ReleaseThis();
            }

            foreach (var timeline in _constraintTimelines)
            {
                timeline.ReleaseThis();
            }

            foreach (var bonePose in _bonePoses.Values)
            {
                bonePose.ReleaseThis();
            }

            if (_actionTimeline != null)
            {
                _actionTimeline.ReleaseThis();
            }

            if (_zOrderTimeline != null)
            {
                _zOrderTimeline.ReleaseThis();
            }

            actionEnabled = false;
            additiveBlending = false;
            displayControl = false;
            resetToPose = false;
            playTimes = 1;
            layer = 0;

            timeScale = 1.0f;
            weight = 1.0f;
            autoFadeOutTime = 0.0f;
            fadeTotalTime = 0.0f;
            name = string.Empty;
            group = string.Empty;

            _timelineDirty = 2;
            _playheadState = 0;
            _fadeState = -1;
            _subFadeState = -1;
            _position = 0.0f;
            _duration = 0.0f;
            _fadeTime = 0.0f;
            _time = 0.0f;
            _fadeProgress = 0.0f;
            _weightResult = 0.0f;
            _blendState.Clear();
            _boneMask.Clear();
            _boneTimelines.Clear();
            _slotTimelines.Clear();
            _constraintTimelines.Clear();
            _bonePoses.Clear();
            Animation = null; //
            _armature = null; //
            _actionTimeline = null; //
            _zOrderTimeline = null;
            _parent = null;
        }

        private void _UpdateTimelines()
        {
            { // Update constraint timelines.
                foreach (var constraint in _armature.Structure.Constraints)
                {
                    var timelineDatas = Animation.GetConstraintTimelines(constraint.name);

                    if (timelineDatas != null)
                    {
                        foreach (var timelineData in timelineDatas)
                        {
                            switch (timelineData.type)
                            {
                                case TimelineType.IKConstraint:
                                    {
                                        var timeline = BorrowObject<IKConstraintTimelineState>();
                                        timeline.constraint = constraint;
                                        timeline.Init(_armature, this, timelineData);
                                        _constraintTimelines.Add(timeline);
                                        break;
                                    }

                                default:
                                    break;
                            }
                        }
                    }
                    else if (resetToPose)
                    { // Pose timeline.
                        var timeline = BorrowObject<IKConstraintTimelineState>();
                        timeline.constraint = constraint;
                        timeline.Init(_armature, this, null);
                        _constraintTimelines.Add(timeline);
                        _poseTimelines.Add(timeline);
                    }
                }
            }
        }

        private void UpdateBoneAndSlotTimelines()
        {
            { // Update bone timelines.
                Dictionary<string, List<BoneTimelineState>> boneTimelines = new Dictionary<string, List<BoneTimelineState>>();

                foreach (var timeline in _boneTimelines)
                {
                    // Create bone timelines map.
                    var timelineName = timeline.bone.name;
                    if (!(boneTimelines.ContainsKey(timelineName)))
                    {
                        boneTimelines[timelineName] = new List<BoneTimelineState>();
                    }

                    boneTimelines[timelineName].Add(timeline);
                }

                foreach (var bone in _armature.Structure.Bones)
                {
                    var timelineName = bone.name;
                    if (!ContainsBoneMask(timelineName))
                    {
                        continue;
                    }

                    var timelineDatas = Animation.GetBoneTimelines(timelineName);
                    
                    if (boneTimelines.ContainsKey(timelineName))
                    {
                        // Remove bone timeline from map.
                        boneTimelines.Remove(timelineName);
                    }
                    else
                    {
                        // Create new bone timeline.
                        var bonePose = _bonePoses.ContainsKey(timelineName) ? _bonePoses[timelineName] : (_bonePoses[timelineName] = BorrowObject<BonePose>());
                        if (timelineDatas != null)
                        {
                            foreach (var timelineData in timelineDatas)
                            {
                                switch (timelineData.type)
                                {
                                    case TimelineType.BoneAll:
                                        {
                                            var timeline = BorrowObject<BoneAllTimelineState>();
                                            timeline.bone = bone;
                                            timeline.bonePose = bonePose;
                                            timeline.Init(_armature, this, timelineData);
                                            _boneTimelines.Add(timeline);
                                            break;
                                        }
                                    case TimelineType.BoneTranslate:
                                        {
                                            var timeline = BorrowObject<BoneTranslateTimelineState>();
                                            timeline.bone = bone;
                                            timeline.bonePose = bonePose;
                                            timeline.Init(_armature, this, timelineData);
                                            _boneTimelines.Add(timeline);
                                            break;
                                        }
                                    case TimelineType.BoneRotate:
                                        {
                                            var timeline = BorrowObject<BoneRotateTimelineState>();
                                            timeline.bone = bone;
                                            timeline.bonePose = bonePose;
                                            timeline.Init(_armature, this, timelineData);
                                            _boneTimelines.Add(timeline);
                                            break;
                                        }
                                    case TimelineType.BoneScale:
                                        {
                                            var timeline = BorrowObject<BoneScaleTimelineState>();
                                            timeline.bone = bone;
                                            timeline.bonePose = bonePose;
                                            timeline.Init(_armature, this, timelineData);
                                            _boneTimelines.Add(timeline);
                                            break;
                                        }

                                    default:
                                        break;
                                }
                            }
                        }
                        else if (resetToPose)
                        { // Pose timeline.
                            var timeline = BorrowObject<BoneAllTimelineState>();
                            timeline.bone = bone;
                            timeline.bonePose = bonePose;
                            timeline.Init(_armature, this, null);
                            _boneTimelines.Add(timeline);
                            _poseTimelines.Add(timeline);
                        }
                    }
                }

                foreach (var timelines in boneTimelines.Values)
                {
                    // Remove bone timelines.
                    foreach (var timeline in timelines)
                    {
                        _boneTimelines.Remove(timeline);
                        timeline.ReleaseThis();
                    }
                }
            }

            { // Update slot timelines.
                Dictionary<string, List<SlotTimelineState>> slotTimelines = new Dictionary<string, List<SlotTimelineState>>();
                List<int> ffdFlags = new List<int>();

                foreach (var timeline in _slotTimelines)
                {
                    // Create slot timelines map.
                    var timelineName = timeline.slot.Name;
                    if (!(slotTimelines.ContainsKey(timelineName)))
                    {
                        slotTimelines[timelineName] = new List<SlotTimelineState>();
                    }

                    slotTimelines[timelineName].Add(timeline);
                }

                foreach (var slot in _armature.Structure.Slots)
                {
                    string boneName = slot.Parent.name;
                    if (!ContainsBoneMask(boneName))
                    {
                        continue;
                    }

                    string timelineName = slot.Name;
                    List<TimelineData> timelineDatas = Animation.GetSlotTimelines(timelineName);

                    if (slotTimelines.ContainsKey(timelineName))
                    {
                        // Remove slot timeline from map.
                        slotTimelines.Remove(timelineName);
                    }
                    else
                    {
                        // Create new slot timeline.
                        var displayIndexFlag = false;
                        var colorFlag = false;
                        ffdFlags.Clear();

                        if (timelineDatas != null)
                        {
                            foreach (var timelineData in timelineDatas)
                            {
                                switch (timelineData.type)
                                {
                                    case TimelineType.SlotDisplay:
                                    {
                                        var timeline = BorrowObject<SlotDisplayTimelineState>();
                                        timeline.slot = slot;
                                        timeline.Init(_armature, this, timelineData);
                                        _slotTimelines.Add(timeline);
                                        displayIndexFlag = true;
                                        break;
                                    }
                                    case TimelineType.SlotColor:
                                    {
                                        var timeline = BorrowObject<SlotColorTimelineState>();
                                        timeline.slot = slot;
                                        timeline.Init(_armature, this, timelineData);
                                        _slotTimelines.Add(timeline);
                                        colorFlag = true;
                                        break;
                                    }
                                    case TimelineType.SlotDeform:
                                    {
                                        var timeline = BorrowObject<DeformTimelineState>();
                                        timeline.slot = slot;
                                        timeline.Init(_armature, this, timelineData);
                                        _slotTimelines.Add(timeline);
                                        ffdFlags.Add((int)timeline.vertexOffset);
                                        break;
                                    }
                                }
                            }
                        }

                        if (resetToPose)
                        {
                            // Pose timeline.
                            if (!displayIndexFlag)
                            {
                                var timeline = BorrowObject<SlotDisplayTimelineState>();
                                timeline.slot = slot;
                                timeline.Init(_armature, this, null);
                                _slotTimelines.Add(timeline);
                                _poseTimelines.Add(timeline);
                            }

                            if (!colorFlag)
                            {
                                var timeline = BorrowObject<SlotColorTimelineState>();
                                timeline.slot = slot;
                                timeline.Init(_armature, this, null);
                                _slotTimelines.Add(timeline);
                                _poseTimelines.Add(timeline);
                            }

                            foreach (var displayData in slot.Displays.GetAllData())
                            {
                                if (displayData.type == DisplayType.Mesh)
                                {
                                    int meshOffset = (displayData as MeshDisplayData).vertices.offset;
                                    
                                    if (!ffdFlags.Contains(meshOffset))
                                    {
                                        var timeline = BorrowObject<DeformTimelineState>();
                                        timeline.vertexOffset = meshOffset; //
                                        timeline.slot = slot;
                                        timeline.Init(_armature, this, null);
                                        _slotTimelines.Add(timeline);
                                        _poseTimelines.Add(timeline);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var timelines in slotTimelines.Values)
                {
                    // Remove slot timelines.
                    foreach (var timeline in timelines)
                    {
                        _slotTimelines.Remove(timeline);
                        timeline.ReleaseThis();
                    }
                }
            }
        }

        private void _AdvanceFadeTime(float passedTime)
        {
            var isFadeOut = _fadeState > 0;

            if (_subFadeState < 0)
            {
                // Fade start event.
                _subFadeState = 0;

                var eventType = isFadeOut ? EventObject.FADE_OUT : EventObject.FADE_IN;
                if (_armature.ArmatureEventDispatcher.HasDBEventListener(eventType))
                {
                    var eventObject = BorrowObject<EventObject>();
                    eventObject.type = eventType;
                    eventObject.armature = _armature;
                    eventObject.animationState = this;
                    DBInitial.Kernel.BufferEvent(eventObject);
                }
            }

            if (passedTime < 0.0f)
            {
                passedTime = -passedTime;
            }

            _fadeTime += passedTime;

            if (_fadeTime >= fadeTotalTime)
            {
                // Fade complete.
                _subFadeState = 1;
                _fadeProgress = isFadeOut ? 0.0f : 1.0f;
            }
            else if (_fadeTime > 0.0f)
            {
                // Fading.
                _fadeProgress = isFadeOut ? (1.0f - _fadeTime / fadeTotalTime) : (_fadeTime / fadeTotalTime);
            }
            else
            {
                // Before fade.
                _fadeProgress = isFadeOut ? 1.0f : 0.0f;
            }

            if (_subFadeState > 0)
            {
                // Fade complete event.
                if (!isFadeOut)
                {
                    _playheadState |= 1; // x1
                    _fadeState = 0;
                }

                var eventType = isFadeOut ? EventObject.FADE_OUT_COMPLETE : EventObject.FADE_IN_COMPLETE;
                if (_armature.ArmatureEventDispatcher.HasDBEventListener(eventType))
                {
                    var eventObject = BorrowObject<EventObject>();
                    eventObject.type = eventType;
                    eventObject.armature = _armature;
                    eventObject.animationState = this;
                    DBInitial.Kernel.BufferEvent(eventObject);
                }
            }
        }

        /// <internal/>
        /// <private/>
        internal void Init(Armature armature, AnimationData animationData, AnimationConfig animationConfig)
        {
            if (_armature != null)
            {
                return;
            }

            _armature = armature;

            Animation = animationData;
            resetToPose = animationConfig.resetToPose;
            additiveBlending = animationConfig.additiveBlending;
            displayControl = animationConfig.displayControl;
            actionEnabled = animationConfig.actionEnabled;
            layer = animationConfig.layer;
            playTimes = animationConfig.playTimes;
            timeScale = animationConfig.timeScale;
            fadeTotalTime = animationConfig.fadeInTime;
            autoFadeOutTime = animationConfig.autoFadeOutTime;
            weight = animationConfig.weight;
            name = animationConfig.name.Length > 0 ? animationConfig.name : animationConfig.animation;
            group = animationConfig.group;

            if (animationConfig.pauseFadeIn)
            {
                _playheadState = 2; // 10
            }
            else
            {
                _playheadState = 3; // 11
            }

            if (animationConfig.duration < 0.0f)
            {
                _position = 0.0f;
                _duration = Animation.duration;
                if (animationConfig.position != 0.0f)
                {
                    if (timeScale >= 0.0f)
                    {
                        _time = animationConfig.position;
                    }
                    else
                    {
                        _time = animationConfig.position - _duration;
                    }
                }
                else
                {
                    _time = 0.0f;
                }
            }
            else
            {
                _position = animationConfig.position;
                _duration = animationConfig.duration;
                _time = 0.0f;
            }

            if (timeScale < 0.0f && _time == 0.0f)
            {
                _time = -0.000001f; // Turn to end.
            }

            if (fadeTotalTime <= 0.0f)
            {
                _fadeProgress = 0.999999f; // Make different.
            }

            if (animationConfig.boneMask.Count > 0)
            {
                _boneMask.ResizeList(animationConfig.boneMask.Count);
                for (int i = 0, l = _boneMask.Count; i < l; ++i)
                {
                    _boneMask[i] = animationConfig.boneMask[i];
                }
            }

            _actionTimeline = BorrowObject<ActionTimelineState>();
            _actionTimeline.Init(_armature, this, Animation.actionTimeline);
            _actionTimeline.currentTime = _time;
            if (_actionTimeline.currentTime < 0.0f)
            {
                _actionTimeline.currentTime = _duration - _actionTimeline.currentTime;
            }

            if (Animation.zOrderTimeline != null)
            {
                _zOrderTimeline = BorrowObject<ZOrderTimelineState>();
                _zOrderTimeline.Init(_armature, this, Animation.zOrderTimeline);
            }
        }
        /// <internal/>
        /// <private/>
        internal void AdvanceTime(float passedTime, float cacheFrameRate)
        {
            _blendState.dirty = true;

            // Update fade time.
            if (_fadeState != 0 || _subFadeState != 0)
            {
                _AdvanceFadeTime(passedTime);
            }

            // Update time.
            if (_playheadState == 3)
            {
                // 11
                if (timeScale != 1.0f)
                {
                    passedTime *= timeScale;
                }

                _time += passedTime;
            }

            // Update timeline.
            if (_timelineDirty != 0)
            {
                if (_timelineDirty == 2)
                {
                    _UpdateTimelines();
                }

                _timelineDirty = 0;
                UpdateBoneAndSlotTimelines();
            }

            if (weight == 0.0f)
            {
                return;
            }

            var isCacheEnabled = _fadeState == 0 && cacheFrameRate > 0.0f;
            var isUpdateTimeline = true;
            var isUpdateBoneTimeline = true;
            var time = _time;
            _weightResult = weight * _fadeProgress;

            if (_parent != null)
            {
                _weightResult *= _parent._weightResult / _parent._fadeProgress;
            }

            if (_actionTimeline.playState <= 0)
            {
                // Update main timeline.
                _actionTimeline.Update(time);
            }

            if (isCacheEnabled)
            {
                // Cache time internval.
                var internval = cacheFrameRate * 2.0f;
                _actionTimeline.currentTime = (float)Math.Floor(_actionTimeline.currentTime * internval) / internval;
            }

            if (_zOrderTimeline != null && _zOrderTimeline.playState <= 0)
            {
                // Update zOrder timeline.
                _zOrderTimeline.Update(time);
            }

            if (isCacheEnabled)
            {
                // Update cache.
                previousCacheFrameIndex = CurrentCacheFrameIndex;
                CurrentCacheFrameIndex = (int)Math.Floor(_actionTimeline.currentTime * cacheFrameRate); // uint
                
                if (previousCacheFrameIndex == CurrentCacheFrameIndex)
                {
                    // Same cache.
                    isUpdateTimeline = false;
                    isUpdateBoneTimeline = false;
                }
                else
                {
                    if (DBInitial.Kernel.Cacher.IsFrameCached(Animation, CurrentCacheFrameIndex))
                    {
                        // Cached.
                        isUpdateBoneTimeline = false;
                    }
                }
            }

            if (isUpdateTimeline)
            {
                if (isUpdateBoneTimeline)
                {
                    for (int i = 0, l = _boneTimelines.Count; i < l; ++i)
                    {
                        var timeline = _boneTimelines[i];

                        if (timeline.playState <= 0)
                        {
                            timeline.Update(time);
                        }

                        if (i == l - 1 || timeline.bone != _boneTimelines[i + 1].bone)
                        {
                            var state = timeline.bone._blendState.Update(_weightResult, layer);
                            if (state != 0)
                            {
                                timeline.Blend(state);
                            }
                        }
                    }
                }

                if (displayControl)
                {
                    for (int i = 0, l = _slotTimelines.Count; i < l; ++i)
                    {
                        var timeline = _slotTimelines[i];
                        var displayController = timeline.slot.displayController;

                        if (
                            displayController == null ||
                            displayController == name ||
                            displayController == group
                        )
                        {
                            if (timeline.playState <= 0)
                            {
                                timeline.Update(time);
                            }
                        }
                    }
                }

                for (int i = 0, l = _constraintTimelines.Count; i < l; ++i)
                {
                    var timeline = _constraintTimelines[i];
                    if (timeline.playState <= 0)
                    {
                        timeline.Update(time);
                    }
                }
            }

            if (_fadeState == 0)
            {
                if (_subFadeState > 0)
                {
                    _subFadeState = 0;

                    if (_poseTimelines.Count > 0)
                    {
                        foreach (var timeline in _poseTimelines)
                        {
                            if (timeline is BoneTimelineState)
                            {
                                _boneTimelines.Remove(timeline as BoneTimelineState);
                            }
                            else if (timeline is SlotTimelineState)
                            {
                                _slotTimelines.Remove(timeline as SlotTimelineState);
                            }
                            else if (timeline is ConstraintTimelineState)
                            {
                                _constraintTimelines.Remove(timeline as ConstraintTimelineState);
                            }

                            timeline.ReleaseThis();
                        }

                        _poseTimelines.Clear();
                    }
                }

                if (_actionTimeline.playState > 0)
                {
                    if (autoFadeOutTime >= 0.0f)
                    {
                        // Auto fade out.
                        FadeOut(autoFadeOutTime);
                    }
                }
            }
        }

        /// <summary>
        /// - Continue play.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public void Play()
        {
            _playheadState = 3; // 11
        }
        /// <summary>
        /// - Stop play.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public void Stop()
        {
            _playheadState &= 1; // 0x
        }
        /// <summary>
        /// - Fade out the animation state.
        /// </summary>
        /// <param name="fadeOutTime">- The fade out time. (In seconds)</param>
        /// <param name="pausePlayhead">- Whether to pause the animation playing when fade out.</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public void FadeOut(float fadeOutTime, bool pausePlayhead = true)
        {
            if (fadeOutTime < 0.0f)
            {
                fadeOutTime = 0.0f;
            }

            if (pausePlayhead)
            {
                _playheadState &= 2; // x0
            }

            if (_fadeState > 0)
            {
                if (fadeOutTime > fadeTotalTime - _fadeTime)
                {
                    // If the animation is already in fade out, the new fade out will be ignored.
                    return;
                }
            }
            else
            {
                _fadeState = 1;
                _subFadeState = -1;

                if (fadeOutTime <= 0.0f || _fadeProgress <= 0.0f)
                {
                    _fadeProgress = 0.000001f; // Modify fade progress to different value.
                }

                foreach (var timeline in _boneTimelines)
                {
                    timeline.FadeOut();
                }

                foreach (var timeline in _slotTimelines)
                {
                    timeline.FadeOut();
                }

                foreach (var timeline in _constraintTimelines)
                {
                    timeline.FadeOut();
                }
            }

            displayControl = false; //
            fadeTotalTime = _fadeProgress > 0.000001f ? fadeOutTime / _fadeProgress : 0.0f;
            _fadeTime = fadeTotalTime * (1.0f - _fadeProgress);
        }

        /// <summary>
        /// - Check if a specific bone mask is included.
        /// </summary>
        /// <param name="boneName">- The bone name.</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public bool ContainsBoneMask(string boneName)
        {
            return _boneMask.Count == 0 || _boneMask.IndexOf(boneName) >= 0;
        }
        /// <summary>
        /// - Add a specific bone mask.
        /// </summary>
        /// <param name="boneName">- The bone name.</param>
        /// <param name="recursive">- Whether or not to add a mask to the bone's sub-bone.</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public void AddBoneMask(string boneName, bool recursive = true)
        {
            var currentBone = _armature.Structure.GetBone(boneName);
            if (currentBone == null)
            {
                return;
            }

            if (_boneMask.IndexOf(boneName) < 0)
            {
                // Add mixing
                _boneMask.Add(boneName);
            }

            if (recursive)
            {
                // Add recursive mixing.
                foreach (var bone in _armature.Structure.Bones)
                {
                    if (_boneMask.IndexOf(bone.name) < 0 && currentBone.Contains(bone))
                    {
                        _boneMask.Add(bone.name);
                    }
                }
            }

            _timelineDirty = 1;
        }
        /// <summary>
        /// - Remove the mask of a specific bone.
        /// </summary>
        /// <param name="boneName">- The bone name.</param>
        /// <param name="recursive">- Whether to remove the bone's sub-bone mask.</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        /// <language>zh_CN</language>
        public void RemoveBoneMask(string boneName, bool recursive = true)
        {
            if (_boneMask.Contains(boneName))
            {
                _boneMask.Remove(boneName);
            }

            if (recursive)
            {
                var currentBone = _armature.Structure.GetBone(boneName);
                if (currentBone != null)
                {
                    var bones = _armature.Structure.Bones;
                    if (_boneMask.Count > 0)
                    {
                        // Remove recursive mixing.
                        foreach (var bone in bones)
                        {
                            if (_boneMask.Contains(bone.name) && currentBone.Contains(bone))
                            {
                                _boneMask.Remove(bone.name);
                            }
                        }
                    }
                    else
                    {
                        // Add unrecursive mixing.
                        foreach (var bone in bones)
                        {
                            if (bone == currentBone)
                            {
                                continue;
                            }

                            if (!currentBone.Contains(bone))
                            {
                                _boneMask.Add(bone.name);
                            }
                        }
                    }
                }
            }

            _timelineDirty = 1;
        }
        /// <summary>
        /// - Remove all bone masks.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public void RemoveAllBoneMask()
        {
            _boneMask.Clear();
            _timelineDirty = 1;
        }
        /// <summary>
        /// - Whether the animation state is fading in.
        /// </summary>
        /// <version>DragonBones 5.1</version>
        /// <language>en_US</language>
        public bool isFadeIn
        {
            get { return _fadeState < 0; }
        }
        /// <summary>
        /// - Whether the animation state is fading out.
        /// </summary>
        /// <version>DragonBones 5.1</version>
        /// <language>en_US</language>
        public bool isFadeOut
        {
            get { return _fadeState > 0; }
        }
        /// <summary>
        /// - Whether the animation state is fade completed.
        /// </summary>
        /// <version>DragonBones 5.1</version>
        /// <language>en_US</language>
        public bool isFadeComplete
        {
            get { return _fadeState == 0; }
        }
        /// <summary>
        /// - Whether the animation state is playing.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public bool isPlaying
        {
            get { return (_playheadState & 2) != 0 && _actionTimeline.playState <= 0; }
        }
        /// <summary>
        /// - Whether the animation state is play completed.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public bool isCompleted
        {
            get { return _actionTimeline.playState > 0; }
        }
        /// <summary>
        /// - The times has been played.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public int currentPlayTimes
        {
            get { return _actionTimeline.currentPlayTimes; }
        }

        /// <summary>
        /// - The total time. (In seconds)
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public float totalTime
        {
            get { return _duration; }
        }
        /// <summary>
        /// - The time is currently playing. (In seconds)
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public float currentTime
        {
            get { return _actionTimeline.currentTime; }
            set
            {
                var currentPlayTimes = _actionTimeline.currentPlayTimes - (_actionTimeline.playState > 0 ? 1 : 0);
                if (value < 0.0f || _duration < value)
                {
                    value = (value % _duration) + currentPlayTimes * _duration;
                    if (value < 0.0f)
                    {
                        value += _duration;
                    }
                }

                if (playTimes > 0 && currentPlayTimes == playTimes - 1 && value == _duration)
                {
                    value = _duration - 0.000001f;
                }

                if (_time == value)
                {
                    return;
                }

                _time = value;
                _actionTimeline.SetCurrentTime(_time);

                if (_zOrderTimeline != null)
                {
                    _zOrderTimeline.playState = -1;
                }

                foreach (var timeline in _boneTimelines)
                {
                    timeline.playState = -1;
                }

                foreach (var timeline in _slotTimelines)
                {
                    timeline.playState = -1;
                }
            }
        }
    }

    /// <internal/>
    /// <private/>
    internal class BonePose : DBObject
    {
        public readonly DBTransform current = new DBTransform();
        public readonly DBTransform delta = new DBTransform();
        public readonly DBTransform result = new DBTransform();

        public override void OnReleased()
        {
            current.Identity();
            delta.Identity();
            result.Identity();
        }
    }

    /// <internal/>
    /// <private/>
    internal class BlendState
    {
        public bool dirty;
        public int layer;
        public float leftWeight;
        public float layerWeight;
        public float blendWeight;

        /// <summary>
        /// -1: First blending, 0: No blending, 1: Blending.
        /// </summary>
        public int Update(float weight, int p_layer)
        {
            if (dirty)
            {
                if (leftWeight > 0.0f)
                {
                    if (layer != p_layer)
                    {
                        if (layerWeight >= leftWeight)
                        {
                            leftWeight = 0.0f;

                            return 0;
                        }
                        else
                        {
                            layer = p_layer;
                            leftWeight -= layerWeight;
                            layerWeight = 0.0f;
                        }
                    }
                }
                else
                {
                    return 0;
                }

                weight *= leftWeight;
                layerWeight += weight;
                blendWeight = weight;

                return 2;
            }

            dirty = true;
            layer = p_layer;
            layerWeight = weight;
            leftWeight = 1.0f;
            blendWeight = weight;

            return 1;
        }

        public void Clear()
        {
            dirty = false;
            layer = 0;
            leftWeight = 0.0f;
            layerWeight = 0.0f;
            blendWeight = 0.0f;
        }
    }
}
