using System.Collections.Generic;

namespace DragonBones
{
    public sealed class DBKernel
    {
        public static readonly string VERSION = "5.6.300";
        
        /// <summary>
        /// Y orientation in game engine
        /// </summary>
        /// <language>en_US</language>
        public static bool IsNegativeYDown = true;

        private readonly List<EventObject> eventsToDispatch = new();
        private readonly List<DBObject> objectsToDispose = new();

        //components
        public DBRegistry Registry { get; private set; } = new();
        public DBDataStorage DataStorage { get; private set; }
        public DBFactory Factory { get; private set; }
        public WorldClock Clock { get; } = new WorldClock();
        public DBFrameCacher Cacher { get; } = new(24);
        
        public IEventDispatcher<EventObject> SoundEventManager { get; }

        public DBKernel(IEventDispatcher<EventObject> soundEventManager, DBFactory factory)
        {
            SoundEventManager = soundEventManager;
            Factory = factory;
            DataStorage = new DBDataStorage();
        }

        public void AdvanceTime(float passedTime)
        {
            DisposeObjects();
            DispatchEvents();

            Clock.AdvanceTime(passedTime);
        }

        private void DispatchEvents()
        {
            if (eventsToDispatch.Count == 0) return;
            
            for (int i = 0; i < eventsToDispatch.Count; i++)
            {
                var eventObject = eventsToDispatch[i];
                var armature = eventObject.armature;
                    
                if (armature.ArmatureData != null)
                {
                    // May be armature disposed before advanceTime.
                    armature.ArmatureEventDispatcher.DispatchDBEvent(eventObject.type, eventObject);
                        
                    if (eventObject.type == EventObject.SOUND_EVENT)
                    {
                        SoundEventManager.DispatchDBEvent(eventObject.type, eventObject);
                    }
                }

                BufferObject(eventObject);
            }

            eventsToDispatch.Clear();
        }

        private void DisposeObjects()
        {
            if (objectsToDispose.Count > 0)
            {
                for (int i = 0; i < objectsToDispose.Count; ++i)
                {
                    var obj = objectsToDispose[i];
                    obj.ReleaseThis();
                }

                objectsToDispose.Clear();
            }
        }

        public void BufferEvent(EventObject value)
        {
            if (!eventsToDispatch.Contains(value))
            {
                eventsToDispatch.Add(value);
            }
        }

        public void BufferObject(DBObject value)
        {
            if (!objectsToDispose.Contains(value))
            {
                objectsToDispose.Add(value);
            }
        }

        public static implicit operator bool(DBKernel exists)
        {
            return exists != null;
        }
    }
    
    enum BinaryOffset
    {
        WeightBoneCount = 0,
        WeightFloatOffset = 1,
        WeightBoneIndices = 2,

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

    public enum ArmatureType
    {
        None = -1,
        Armature = 0,
        MovieClip = 1,
        Stage = 2
    }
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

    public enum ActionType
    {
        Play = 0,
        Frame = 10,
        Sound = 11
    }

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

    public enum TweenType
    {
        None = 0,
        Line = 1,
        Curve = 2,
        QuadIn = 3,
        QuadOut = 4,
        QuadInOut = 5
    }

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
}