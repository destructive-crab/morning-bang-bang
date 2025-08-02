using System;
using System.Collections.Generic;

namespace DragonBones
{
    /// <summary>
    /// - Worldclock provides clock support for animations, advance time for each IAnimatable object added to the instance.
    /// </summary>
    /// <see cref="DBKernel.IAnimateble"/>
    /// <see cref="DBKernel.Armature"/>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public class WorldClock : IAnimatable
    {
        /// <summary>
        /// - Current time. (In seconds)
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public float time = 0.0f;

        /// <summary>
        /// - The play speed, used to control animation speed-shift play.
        /// [0: Stop play, (0~1): Slow play, 1: Normal play, (1~N): Fast play]
        /// </summary>
        /// <default>1.0</default>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public float timeScale = 1.0f;
        private float _systemTime = 0.0f;
        private readonly List<IAnimatable> _animatebles = new List<IAnimatable>();
        private WorldClock _clock = null;
        /// <summary>
        /// - Creating a Worldclock instance. Typically, you do not need to create Worldclock instance.
        /// When multiple Worldclock instances are running at different speeds, can achieving some specific animation effects, such as bullet time.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public WorldClock(float time = -1.0f)
        {
            this.time = time;
            this._systemTime = DateTime.Now.Ticks * 0.01f * 0.001f;
        }

        /// <summary>
        /// - Advance time for all IAnimatable instances.
        /// </summary>
        /// <param name="passedTime">- Passed time. [-1: Automatically calculates the time difference between the current frame and the previous frame, [0~N): Passed time] (In seconds)</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public void AdvanceTime(float passedTime)
        {
            if (float.IsNaN(passedTime))
            {
                passedTime = 0.0f;
            }

            var currentTime = DateTime.Now.Ticks * 0.01f * 0.001f;
            if (passedTime < 0.0f)
            {
                passedTime = currentTime - this._systemTime;
            }

            this._systemTime = currentTime;

            if (this.timeScale != 1.0f)
            {
                passedTime *= this.timeScale;
            }

            if (passedTime == 0.0f)
            {
                return;
            }

            if (passedTime < 0.0f)
            {
                this.time -= passedTime;
            }
            else
            {
                this.time += passedTime;
            }

            int i = 0, r = 0, l = _animatebles.Count;
            for (; i < l; ++i)
            {
                var animateble = _animatebles[i];
                if (animateble != null)
                {
                    if (r > 0)
                    {
                        _animatebles[i - r] = animateble;
                        _animatebles[i] = null;
                    }

                    animateble.AdvanceTime(passedTime);
                }
                else
                {
                    r++;
                }
            }

            if (r > 0)
            {
                l = _animatebles.Count;
                for (; i < l; ++i)
                {
                    var animateble = _animatebles[i];
                    if (animateble != null)
                    {
                        _animatebles[i - r] = animateble;
                    }
                    else
                    {
                        r++;
                    }
                }

                _animatebles.ResizeList(l - r, null);
            }
        }
        /// <summary>
        /// - Check whether contains a specific instance of IAnimatable.
        /// </summary>
        /// <param name="value">- The IAnimatable instance.</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public bool Contains(IAnimatable value)
        {
            if (value == this)
            {
                return false;
            }

            IAnimatable ancestor = value;
            while (ancestor != this && ancestor != null)
            {
                ancestor = ancestor.clock;
            }

            return ancestor == this;
        }
        /// <summary>
        /// - Add IAnimatable instance.
        /// </summary>
        /// <param name="value">- The IAnimatable instance.</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public void Add(IAnimatable value)
        {
            if (value != null && !_animatebles.Contains(value))
            {
                _animatebles.Add(value);
                value.clock = this;
            }
        }
        /// <summary>
        /// - Removes a specified IAnimatable instance.
        /// </summary>
        /// <param name="value">- The IAnimatable instance.</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public void Remove(IAnimatable value)
        {
            var index = _animatebles.IndexOf(value);
            if (index >= 0)
            {
                _animatebles[index] = null;
                value.clock = null;
            }
        }
        /// <summary>
        /// - Clear all IAnimatable instances.
        /// </summary>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        public void Clear()
        {
            for (int i = 0, l = _animatebles.Count; i < l; ++i)
            {
                var animateble = _animatebles[i];
                _animatebles[i] = null;
                if (animateble != null)
                {
                    animateble.clock = null;
                }
            }
        }
        /// <summary>
        /// - Deprecated, please refer to {@link dragonBones.BaseFactory#clock}.
        /// </summary>
        /// <language>en_US</language>
        [System.Obsolete("")]
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
            }
        }
    }
}