using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace DragonBones
{
    /// <internal/>
    /// <private/>
    enum BinaryOffset
    {
        WeigthBoneCount = 0,
        WeigthFloatOffset = 1,
        WeigthBoneIndices = 2,

        MeshVertexCount = 0,
        MeshTriangleCount = 1,
        MeshFloatOffset = 2,
        MeshWeightOffset = 3,
        MeshVertexIndices = 4,

        TimelineScale = 0,
        TimelineOffset = 1,
        TimelineKeyFrameCount = 2,
        TimelineFrameValueCount = 3,
        TimelineFrameValueOffset = 4,
        TimelineFrameOffset = 5,

        FramePosition = 0,
        FrameTweenType = 1,
        FrameTweenEasingOrCurveSampleCount = 2,
        FrameCurveSamples = 3,

        DeformVertexOffset = 0,
        DeformCount = 1,
        DeformValueCount = 2,
        DeformValueOffset = 3,
        DeformFloatOffset = 4
    }

    /// <internal/>
    /// <private/>
    public enum ArmatureType
    {
        None = -1,
        Armature = 0,
        MovieClip = 1,
        Stage = 2
    }
    /// <private/>
    public enum DisplayType
    {
        None = -1,
        Image = 0,
        Armature = 1,
        Mesh = 2,
        BoundingBox = 3,
        Path = 4
    }
    /// <summary>
    /// - Bounding box type.
    /// </summary>
    /// <version>DragonBones 5.0</version>
    /// <language>en_US</language>
    public enum BoundingBoxType
    {
        None = -1,
        Rectangle = 0,
        Ellipse = 1,
        Polygon = 2
    }
    /// <internal/>
    /// <private/>
    public enum ActionType
    {
        Play = 0,
        Frame = 10,
        Sound = 11
    }
    /// <internal/>
    /// <private/>
    public enum BlendMode
    {
        Normal = 0,
        Add = 1,
        Alpha = 2,
        Darken = 3,
        Difference = 4,
        Erase = 5,
        HardLight = 6,
        Invert = 7,
        Layer = 8,
        Lighten = 9,
        Multiply = 10,
        Overlay = 11,
        Screen = 12,
        Subtract = 13
    }

    /// <internal/>
    /// <private/>
    public enum TweenType
    {
        None = 0,
        Line = 1,
        Curve = 2,
        QuadIn = 3,
        QuadOut = 4,
        QuadInOut = 5
    }

    /// <internal/>
    /// <private/>
    public enum TimelineType
    {
        Action = 0,
        ZOrder = 1,

        BoneAll = 10,
        BoneTranslate = 11,
        BoneRotate = 12,
        BoneScale = 13,

        SlotDisplay = 20,
        SlotColor = 21,
        SlotDeform = 22,

        IKConstraint = 30,

        AnimationTime = 40,
        AnimationWeight = 41
    }
    /// <summary>
    /// - Offset mode.
    /// </summary>
    /// <version>DragonBones 5.5</version>
    /// <language>en_US</language>
    public enum OffsetMode
    {
        None,
        Additive,
        Override
    }
    /// <summary>
    /// - Animation fade out mode.
    /// </summary>
    /// <version>DragonBones 4.5</version>
    /// <language>en_US</language>
    public enum AnimationFadeOutMode
    {
        /// <summary>
        /// - Do not fade out of any animation states.
        /// </summary>
        /// <language>en_US</language>
        None = 0,
        /// <summary>
        /// - Fade out the animation states of the same layer.
        /// </summary>
        /// <language>en_US</language>
        SameLayer = 1,
        /// <summary>
        /// - Fade out the animation states of the same group.
        /// </summary>
        /// <language>en_US</language>
        SameGroup = 2,
        /// <summary>
        /// - Fade out the animation states of the same layer and group.
        /// </summary>
        /// <language>en_US</language>
        SameLayerAndGroup = 3,
        /// <summary>
        /// - Fade out of all animation states.
        /// </summary>
        /// <language>en_US</language>
        All = 4,
        /// <summary>
        /// - Does not replace the animation state with the same name.
        /// </summary>
        /// <language>en_US</language>
        Single = 5
    }

    internal static class Helper
    {
        public static readonly int INT16_SIZE = 2;
        public static readonly int UINT16_SIZE = 2;
        public static readonly int FLOAT_SIZE = 4;

        internal static void Assert(bool condition, string message)
        {
            Debug.Assert(condition, message);
        }

        internal static void ResizeList<T>(this List<T> list, int count, T value = default(T))
        {
            if (list.Count == count)
            {
                return;
            }

            if (list.Count > count)
            {
                list.RemoveRange(count, list.Count - count);
            }
            else
            {
                //fixed gc,may be memory will grow
                //list.Capacity = count;
                for (int i = list.Count, l = count; i < l; ++i)
                {
                    list.Add(value);
                }
            }
        }

        internal static List<float> Convert(this List<object> list)
        {
            List<float> res = new List<float>();

            for (int i = 0; i < list.Count; i++)
            {
                res[i] = float.Parse(list[i].ToString());
            }

            return res;
        }
        internal static bool FloatEqual(float f0, float f1)
        {
            float f = Math.Abs(f0 - f1);

            return (f < 0.000000001f);
        }
    }

    /// <private/>
    public class DragonBones
    {
        public static bool yDown = true;
        public static bool debug = false;
        public static bool debugDraw = false;
        public static readonly string VERSION = "5.6.300";

        private readonly List<EventObject> _events = new List<EventObject>();
        private readonly List<BaseObject> _objects = new List<BaseObject>();

        public WorldClock Clock { get; } = new WorldClock();
        public IEventDispatcher<EventObject> EventManager { get; } = null;

        public DragonBones(IEventDispatcher<EventObject> eventManager)
        {
            this.EventManager = eventManager;
        }

        public void AdvanceTime(float passedTime)
        {
            if (this._objects.Count > 0)
            {
                for (int i = 0; i < this._objects.Count; ++i)
                {
                    var obj = this._objects[i];
                    obj.ReturnToPool();
                }

                this._objects.Clear();
            }

            if (this._events.Count > 0)
            {
                for (int i = 0; i < this._events.Count; ++i)
                {
                    var eventObject = this._events[i];
                    var armature = eventObject.armature;
                    if (armature._armatureData != null)
                    {
                        // May be armature disposed before advanceTime.
                        armature.eventDispatcher.DispatchDBEvent(eventObject.type, eventObject);
                        if (eventObject.type == EventObject.SOUND_EVENT)
                        {
                            this.EventManager.DispatchDBEvent(eventObject.type, eventObject);
                        }
                    }

                    this.BufferObject(eventObject);
                }

                this._events.Clear();
            }

            this.Clock.AdvanceTime(passedTime);
        }

        public void BufferEvent(EventObject value)
        {
            if (!this._events.Contains(value))
            {
                this._events.Add(value);
            }
        }

        public void BufferObject(BaseObject value)
        {
            if (!this._objects.Contains(value))
            {
                this._objects.Add(value);
            }
        }

        public static implicit operator bool(DragonBones exists)
        {
            return exists != null;
        }
    }
}