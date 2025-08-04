using System.Collections.Generic;

namespace DragonBones
{
    /// <summary>
    /// - The animation player is used to play the animation data and manage the animation states.
    /// </summary>
    /// <see cref="AnimationData"/>, <see cref="AnimationState"/>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public class AnimationPlayer 
    {
        /// <summary>
        /// - The play speed of all animations. [0: Stop, (0~1): Slow, 1: Normal, (1~N): Fast]
        /// </summary>
        /// <default>1.0</default>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public float timeScale;

        private bool _lockUpdate;

        // Update bones and slots cachedFrameIndices.
        private bool _animationDirty;
        private float _inheritTimeScale;
        private readonly List<string> _animationNames = new List<string>();
        private readonly List<AnimationState> _animationStates = new List<AnimationState>();
        private readonly Dictionary<string, AnimationData> _animations = new Dictionary<string, AnimationData>();
        public readonly Armature BelongsTo;
        private AnimationConfig _animationConfig = null; // Initial value.
        private AnimationState _lastAnimationState;

        public AnimationPlayer(Armature belongsTo)
        {
            BelongsTo = belongsTo;
        }

        public void Clear()
        {
            foreach (var animationState in _animationStates)
            {
                animationState.ReturnToPool();
            }

            if (_animationConfig != null)
            {
                _animationConfig.ReturnToPool();
            }

            timeScale = 1.0f;

            _lockUpdate = false;

            _animationDirty = false;
            _inheritTimeScale = 1.0f;
            _animationNames.Clear();
            _animationStates.Clear();
            _animations.Clear();
            _animationConfig = null; //
            _lastAnimationState = null;
        }

        internal void Init()
        {
            _animationConfig = DBObject.BorrowObject<AnimationConfig>();
        }

        internal void AdvanceTime(float passedTime)
        {
            if (passedTime < 0.0f)
            {
                // Only animationState can reverse play.
                passedTime = -passedTime;
            }

            if (BelongsTo.inheritAnimation && BelongsTo.Parent != null)
            {
                // Inherit parent animation timeScale.
                _inheritTimeScale = BelongsTo.Parent.Armature.AnimationPlayer._inheritTimeScale * timeScale;
            }
            else
            {
                _inheritTimeScale = timeScale;
            }

            if (_inheritTimeScale != 1.0f)
            {
                passedTime *= _inheritTimeScale;
            }

            var animationStateCount = _animationStates.Count;
            if (animationStateCount == 1)
            {
                var animationState = _animationStates[0];
                if (animationState._fadeState > 0 && animationState._subFadeState > 0)
                {
                    DBInitial.Kernel.BufferObject(animationState);
                    _animationStates.Clear();
                    _lastAnimationState = null;
                }
                else
                {
                    var animationData = animationState._animationData;
                    var cacheFrameRate = animationData.cacheFrameRate;

                    if (_animationDirty && cacheFrameRate > 0.0f)
                    {
                        // Update cachedFrameIndices.
                        _animationDirty = false;
                        foreach (var bone in BelongsTo.Structure.Bones)
                        {
                            bone._cachedFrameIndices = animationData.GetBoneCachedFrameIndices(bone.name);
                        }

                        foreach (var slot in BelongsTo.Structure.Slots)
                        {
                            var rawDisplayDatas = slot.AllDisplaysData;
                            if (rawDisplayDatas != null && rawDisplayDatas.Count > 0)
                            {
                                var rawDsplayData = rawDisplayDatas[0];
                                if (rawDsplayData != null)
                                {
                                    if (rawDsplayData.parent == BelongsTo.ArmatureData.defaultSkin)
                                    {
                                        slot._cachedFrameIndices = animationData.GetSlotCachedFrameIndices(slot.name);
                                        continue;
                                    }
                                }
                            }

                            slot._cachedFrameIndices = null;
                        }
                    }

                    animationState.AdvanceTime(passedTime, cacheFrameRate);
                }
            }
            else if (animationStateCount > 1)
            {
                for (int i = 0, r = 0; i < animationStateCount; ++i)
                {
                    var animationState = _animationStates[i];
                    if (animationState._fadeState > 0 && animationState._subFadeState > 0)
                    {
                        r++;
                        DBInitial.Kernel.BufferObject(animationState);
                        _animationDirty = true;
                        if (_lastAnimationState == animationState)
                        {
                            // Update last animation state.
                            _lastAnimationState = null;
                        }
                    }
                    else
                    {
                        if (r > 0)
                        {
                            _animationStates[i - r] = animationState;
                        }

                        animationState.AdvanceTime(passedTime, 0.0f);
                    }

                    if (i == animationStateCount - 1 && r > 0)
                    {
                        // Modify animation states size.
                        _animationStates.ResizeList(_animationStates.Count - r);
                        if (_lastAnimationState == null && _animationStates.Count > 0)
                        {
                            _lastAnimationState = _animationStates[_animationStates.Count - 1];
                        }
                    }
                }

                BelongsTo._cacheFrameIndex = -1;
            }
            else
            {
                BelongsTo._cacheFrameIndex = -1;
            }
        }

        private void _FadeOut(AnimationConfig animationConfig)
        {
            switch (animationConfig.fadeOutMode)
            {
                case AnimationFadeOutMode.SameLayer:
                    foreach (var animationState in _animationStates)
                    {
                        if (animationState._parent != null)
                        {
                            continue;
                        }

                        if (animationState.layer == animationConfig.layer)
                        {
                            animationState.FadeOut(animationConfig.fadeOutTime, animationConfig.pauseFadeOut);
                        }
                    }
                    break;
                case AnimationFadeOutMode.SameGroup:
                    foreach (var animationState in _animationStates)
                    {
                        if (animationState._parent != null)
                        {
                            continue;
                        }

                        if (animationState.group == animationConfig.group)
                        {
                            animationState.FadeOut(animationConfig.fadeOutTime, animationConfig.pauseFadeOut);
                        }
                    }
                    break;
                case AnimationFadeOutMode.SameLayerAndGroup:
                    foreach (var animationState in _animationStates)
                    {
                        if (animationState._parent != null)
                        {
                            continue;
                        }

                        if (animationState.layer == animationConfig.layer &&
                            animationState.group == animationConfig.group)
                        {
                            animationState.FadeOut(animationConfig.fadeOutTime, animationConfig.pauseFadeOut);
                        }
                    }
                    break;
                case AnimationFadeOutMode.All:
                    foreach (var animationState in _animationStates)
                    {
                        if (animationState._parent != null)
                        {
                            continue;
                        }

                        animationState.FadeOut(animationConfig.fadeOutTime, animationConfig.pauseFadeOut);
                    }
                    break;
                case AnimationFadeOutMode.None:
                case AnimationFadeOutMode.Single:
                default:
                    break;
            }
        }

        /// <summary>
        /// - Clear all animations states.
        /// </summary>
        /// <see cref="DBKernel.AnimationState"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        public void Reset()
        {
            foreach (var animationState in _animationStates)
            {
                animationState.ReturnToPool();
            }

            _animationDirty = false;
            _animationConfig.Clear();
            _animationStates.Clear();
            _lastAnimationState = null;
        }
        /// <summary>
        /// - Pause a specific animation state.
        /// </summary>
        /// <param name="animationName">- The name of animation state. (If not set, it will pause all animations)</param>
        /// <see cref="DBKernel.AnimationState"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>

        public void Stop(string animationName = null)
        {
            if (animationName != null)
            {
                var animationState = GetState(animationName);
                if (animationState != null)
                {
                    animationState.Stop();
                }
            }
            else
            {
                foreach (var animationState in _animationStates)
                {
                    animationState.Stop();
                }
            }
        }
        /// <summary>
        /// - Play animation with a specific animation config.
        /// The API is still in the experimental phase and may encounter bugs or stability or compatibility issues when used.
        /// </summary>
        /// <param name="animationConfig">- The animation config.</param>
        /// <returns>The playing animation state.</returns>
        /// <see cref="DBKernel.AnimationConfig"/>
        /// <beta/>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>

        public AnimationState PlayConfig(AnimationConfig animationConfig)
        {
            var animationName = animationConfig.animation;
            if (!(_animations.ContainsKey(animationName)))
            {
                DBLogger.Assert(false,
                    "Non-existent animation.\n" +
                    "DragonBones name: " + BelongsTo.ArmatureData.parent.name +
                    "Armature name: " + BelongsTo.Name +
                    "Animation name: " + animationName
                );

                return null;
            }

            var animationData = _animations[animationName];

            if (animationConfig.fadeOutMode == AnimationFadeOutMode.Single)
            {
                foreach (var aniState in _animationStates)
                {
                    if (aniState._animationData == animationData)
                    {
                        return aniState;
                    }
                }
            }

            if (_animationStates.Count == 0)
            {
                animationConfig.fadeInTime = 0.0f;
            }
            else if (animationConfig.fadeInTime < 0.0f)
            {
                animationConfig.fadeInTime = animationData.fadeInTime;
            }

            if (animationConfig.fadeOutTime < 0.0f)
            {
                animationConfig.fadeOutTime = animationConfig.fadeInTime;
            }

            if (animationConfig.timeScale <= -100.0f)
            {
                animationConfig.timeScale = 1.0f / animationData.scale;
            }

            if (animationData.frameCount > 1)
            {
                if (animationConfig.position < 0.0f)
                {
                    animationConfig.position %= animationData.duration;
                    animationConfig.position = animationData.duration - animationConfig.position;
                }
                else if (animationConfig.position == animationData.duration)
                {
                    animationConfig.position -= 0.000001f; // Play a little time before end.
                }
                else if (animationConfig.position > animationData.duration)
                {
                    animationConfig.position %= animationData.duration;
                }

                if (animationConfig.duration > 0.0f && animationConfig.position + animationConfig.duration > animationData.duration)
                {
                    animationConfig.duration = animationData.duration - animationConfig.position;
                }

                if (animationConfig.playTimes < 0)
                {
                    animationConfig.playTimes = (int)animationData.playTimes;
                }
            }
            else
            {
                animationConfig.playTimes = 1;
                animationConfig.position = 0.0f;
                if (animationConfig.duration > 0.0)
                {
                    animationConfig.duration = 0.0f;
                }
            }

            if (animationConfig.duration == 0.0f)
            {
                animationConfig.duration = -1.0f;
            }

            _FadeOut(animationConfig);

            var animationState = DBObject.BorrowObject<AnimationState>();
            animationState.Init(BelongsTo, animationData, animationConfig);
            _animationDirty = true;
            BelongsTo._cacheFrameIndex = -1;

            if (_animationStates.Count > 0)
            {
                var added = false;
                for (int i = 0, l = _animationStates.Count; i < l; ++i)
                {
                    if (animationState.layer > _animationStates[i].layer)
                    {
                        added = true;
                        _animationStates.Insert(i, animationState);
                        break;
                    }
                    else if (i != l - 1 && animationState.layer > _animationStates[i + 1].layer)
                    {
                        added = true;
                        _animationStates.Insert(i + 1, animationState);
                        break;
                    }
                }

                if (!added)
                {
                    _animationStates.Add(animationState);
                }
            }
            else
            {
                _animationStates.Add(animationState);
            }

            // Child armature play same name animation.
            foreach (var slot in BelongsTo.Structure.Slots)
            {
                var childArmature = slot.ChildArmature;
                if (childArmature != null &&
                    childArmature.inheritAnimation &&
                    childArmature.AnimationPlayer.HasAnimation(animationName) &&
                    childArmature.AnimationPlayer.GetState(animationName) == null)
                {
                    childArmature.AnimationPlayer.FadeIn(animationName); //
                }
            }

            if (!_lockUpdate)
            {
                if (animationConfig.fadeInTime <= 0.0f)
                {
                    // Blend animation state, update armature.
                    BelongsTo.AdvanceTime(0.0f);
                }
            }

            _lastAnimationState = animationState;

            return animationState;
        }
        /// <summary>
        /// - Play a specific animation.
        /// </summary>
        /// <param name="animationName">- The name of animation data. (If not set, The default animation will be played, or resume the animation playing from pause status, or replay the last playing animation)</param>
        /// <param name="playTimes">- Playing repeat times. [-1: Use default value of the animation data, 0: No end loop playing, [1~N]: Repeat N times] (default: -1)</param>
        /// <returns>The playing animation state.</returns>
        /// <example>
        /// TypeScript style, for reference only.
        /// <pre>
        ///     armature.animation.play("walk");
        /// </pre>
        /// </example>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public AnimationState Play(string animationName = null, int playTimes = -1)
        {
            _animationConfig.Clear();
            _animationConfig.resetToPose = true;
            _animationConfig.playTimes = playTimes;
            _animationConfig.fadeInTime = 0.0f;
            _animationConfig.animation = animationName != null ? animationName : "";

            if (animationName != null && animationName.Length > 0)
            {
                PlayConfig(_animationConfig);
            }
            else if (_lastAnimationState == null)
            {
                var defaultAnimation = BelongsTo.ArmatureData.defaultAnimation;
                if (defaultAnimation != null)
                {
                    _animationConfig.animation = defaultAnimation.name;
                    PlayConfig(_animationConfig);
                }
            }
            else if (!_lastAnimationState.isPlaying && !_lastAnimationState.isCompleted)
            {
                _lastAnimationState.Play();
            }
            else
            {
                _animationConfig.animation = _lastAnimationState.name;
                PlayConfig(_animationConfig);
            }

            return _lastAnimationState;
        }
        /// <summary>
        /// - Fade in a specific animation.
        /// </summary>
        /// <param name="animationName">- The name of animation data.</param>
        /// <param name="fadeInTime">- The fade in time. [-1: Use the default value of animation data, [0~N]: The fade in time (In seconds)] (Default: -1)</param>
        /// <param name="playTimes">- playing repeat times. [-1: Use the default value of animation data, 0: No end loop playing, [1~N]: Repeat N times] (Default: -1)</param>
        /// <param name="layer">- The blending layer, the animation states in high level layer will get the blending weights with high priority, when the total blending weights are more than 1.0, there will be no more weights can be allocated to the other animation states. (Default: 0)</param>
        /// <param name="group">- The blending group name, it is typically used to specify the substitution of multiple animation states blending. (Default: null)</param>
        /// <param name="fadeOutMode">- The fade out mode, which is typically used to specify alternate mode of multiple animation states blending. (Default: AnimationFadeOutMode.SameLayerAndGroup)</param>
        /// <returns>The playing animation state.</returns>
        /// <example>
        /// TypeScript style, for reference only.
        /// <pre>
        ///     armature.animation.fadeIn("walk", 0.3, 0, 0, "normalGroup").resetToPose = false;
        ///     armature.animation.fadeIn("attack", 0.3, 1, 0, "attackGroup").resetToPose = false;
        /// </pre>
        /// </example>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public AnimationState FadeIn(string animationName, float fadeInTime = -1.0f, int playTimes = -1,
                                    int layer = 0, string group = null,
                                    AnimationFadeOutMode fadeOutMode = AnimationFadeOutMode.SameLayerAndGroup)
        {
            _animationConfig.Clear();
            _animationConfig.fadeOutMode = fadeOutMode;
            _animationConfig.playTimes = playTimes;
            _animationConfig.layer = layer;
            _animationConfig.fadeInTime = fadeInTime;
            _animationConfig.animation = animationName;
            _animationConfig.group = group != null ? group : "";

            return PlayConfig(_animationConfig);
        }
        /// <summary>
        /// - Play a specific animation from the specific time.
        /// </summary>
        /// <param name="animationName">- The name of animation data.</param>
        /// <param name="time">- The start time point of playing. (In seconds)</param>
        /// <param name="playTimes">- Playing repeat times. [-1: Use the default value of animation data, 0: No end loop playing, [1~N]: Repeat N times] (Default: -1)</param>
        /// <returns>The played animation state.</returns>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public AnimationState GotoAndPlayByTime(string animationName, float time = 0.0f, int playTimes = -1)
        {
            _animationConfig.Clear();
            _animationConfig.resetToPose = true;
            _animationConfig.playTimes = playTimes;
            _animationConfig.position = time;
            _animationConfig.fadeInTime = 0.0f;
            _animationConfig.animation = animationName;

            return PlayConfig(_animationConfig);
        }
        /// <summary>
        /// - Play a specific animation from the specific frame.
        /// </summary>
        /// <param name="animationName">- The name of animation data.</param>
        /// <param name="frame">- The start frame of playing.</param>
        /// <param name="playTimes">- Playing repeat times. [-1: Use the default value of animation data, 0: No end loop playing, [1~N]: Repeat N times] (Default: -1)</param>
        /// <returns>The played animation state.</returns>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public AnimationState GotoAndPlayByFrame(string animationName, uint frame = 0, int playTimes = -1)
        {
            _animationConfig.Clear();
            _animationConfig.resetToPose = true;
            _animationConfig.playTimes = playTimes;
            _animationConfig.fadeInTime = 0.0f;
            _animationConfig.animation = animationName;

            var animationData = _animations.ContainsKey(animationName) ? _animations[animationName] : null;
            if (animationData != null)
            {
                _animationConfig.position = animationData.duration * frame / animationData.frameCount;
            }

            return PlayConfig(_animationConfig);
        }
        /// <summary>
        /// - Play a specific animation from the specific progress.
        /// </summary>
        /// <param name="animationName">- The name of animation data.</param>
        /// <param name="progress">- The start progress value of playing.</param>
        /// <param name="playTimes">- Playing repeat times. [-1: Use the default value of animation data, 0: No end loop playing, [1~N]: Repeat N times] (Default: -1)</param>
        /// <returns>The played animation state.</returns>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public AnimationState GotoAndPlayByProgress(string animationName, float progress = 0.0f, int playTimes = -1)
        {
            _animationConfig.Clear();
            _animationConfig.resetToPose = true;
            _animationConfig.playTimes = playTimes;
            _animationConfig.fadeInTime = 0.0f;
            _animationConfig.animation = animationName;

            var animationData = _animations.ContainsKey(animationName) ? _animations[animationName] : null;
            if (animationData != null)
            {
                _animationConfig.position = animationData.duration * (progress > 0.0f ? progress : 0.0f);
            }

            return PlayConfig(_animationConfig);
        }
        /// <summary>
        /// - Stop a specific animation at the specific time.
        /// </summary>
        /// <param name="animationName">- The name of animation data.</param>
        /// <param name="time">- The stop time. (In seconds)</param>
        /// <returns>The played animation state.</returns>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public AnimationState GotoAndStopByTime(string animationName, float time = 0.0f)
        {
            var animationState = GotoAndPlayByTime(animationName, time, 1);
            if (animationState != null)
            {
                animationState.Stop();
            }

            return animationState;
        }
        /// <summary>
        /// - Stop a specific animation at the specific frame.
        /// </summary>
        /// <param name="animationName">- The name of animation data.</param>
        /// <param name="frame">- The stop frame.</param>
        /// <returns>The played animation state.</returns>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public AnimationState GotoAndStopByFrame(string animationName, uint frame = 0)
        {
            var animationState = GotoAndPlayByFrame(animationName, frame, 1);
            if (animationState != null)
            {
                animationState.Stop();
            }

            return animationState;
        }
        /// <summary>
        /// - Stop a specific animation at the specific progress.
        /// </summary>
        /// <param name="animationName">- The name of animation data.</param>
        /// <param name="progress">- The stop progress value.</param>
        /// <returns>The played animation state.</returns>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public AnimationState GotoAndStopByProgress(string animationName, float progress = 0.0f)
        {
            var animationState = GotoAndPlayByProgress(animationName, progress, 1);
            if (animationState != null)
            {
                animationState.Stop();
            }

            return animationState;
        }
        /// <summary>
        /// - Get a specific animation state.
        /// </summary>
        /// <param name="animationName">- The name of animation state.</param>
        /// <example>
        /// TypeScript style, for reference only.
        /// <pre>
        ///     armature.animation.play("walk");
        ///     let walkState = armature.animation.getState("walk");
        ///     walkState.timeScale = 0.5;
        /// </pre>
        /// </example>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public AnimationState GetState(string animationName)
        {
            var i = _animationStates.Count;
            while (i-- > 0)
            {
                var animationState = _animationStates[i];
                if (animationState.name == animationName)
                {
                    return animationState;
                }
            }

            return null;
        }
        /// <summary>
        /// - Check whether a specific animation data is included.
        /// </summary>
        /// <param name="animationName">- The name of animation data.</param>
        /// <see cref="DBKernel.AnimationData"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public bool HasAnimation(string animationName)
        {
            return _animations.ContainsKey(animationName);
        }
        /// <summary>
        /// - Get all the animation states.
        /// </summary>
        /// <version>DragonBones 5.1</version>
        /// <language>en_US</language>
        public List<AnimationState> GetStates()
        {
            return _animationStates;
        }
        /// <summary>
        /// - Check whether there is an animation state is playing
        /// </summary>
        /// <see cref="DBKernel.AnimationState"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public bool isPlaying
        {
            get
            {
                foreach (var animationState in _animationStates)
                {
                    if (animationState.isPlaying)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        /// <summary>
        /// - Check whether all the animation states' playing were finished.
        /// </summary>
        /// <see cref="DBKernel.AnimationState"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public bool isCompleted
        {
            get
            {
                foreach (var animationState in _animationStates)
                {
                    if (!animationState.isCompleted)
                    {
                        return false;
                    }
                }

                return _animationStates.Count > 0;
            }
        }
        /// <summary>
        /// - The name of the last playing animation state.
        /// </summary>
        /// <see cref="lastAnimationState"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public string lastAnimationName
        {
            get { return _lastAnimationState != null ? _lastAnimationState.name : ""; }
        }
        /// <summary>
        /// - The name of all animation data
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public List<string> animationNames
        {
            get { return _animationNames; }
        }
        /// <summary>
        /// - All animation data.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public Dictionary<string, AnimationData> animations
        {
            get { return _animations; }
            set
            {
                if (_animations == value)
                {
                    return;
                }

                _animationNames.Clear();

                _animations.Clear();

                foreach (var k in value)
                {
                    _animationNames.Add(k.Key);
                    _animations[k.Key] = value[k.Key];
                }
            }
        }
        /// <summary>
        /// - An AnimationConfig instance that can be used quickly.
        /// </summary>
        /// <see cref="DBKernel.AnimationConfig"/>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public AnimationConfig animationConfig
        {
            get
            {
                _animationConfig.Clear();
                return _animationConfig;
            }
        }
        /// <summary>
        /// - The last playing animation state
        /// </summary>
        /// <see cref="DBKernel.AnimationState"/>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public AnimationState lastAnimationState
        {
            get { return _lastAnimationState; }
        }
    }
}
