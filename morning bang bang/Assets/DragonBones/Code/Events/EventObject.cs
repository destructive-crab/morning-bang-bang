namespace DragonBones
{
    /// <summary>
    /// - The properties of the object carry basic information about an event,
    /// which are passed as parameter or parameter's parameter to event listeners when an event occurs.
    /// </summary>
    /// <version>DragonBones 4.5</version>
    /// <language>en_US</language>
    public class EventObject : DBObject
    {
        /// <summary>
        /// - Animation start play.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public const string START = "start";
        /// <summary>
        /// - Animation loop play complete once.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public const string LOOP_COMPLETE = "loopComplete";
        /// <summary>
        /// - Animation play complete.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public const string COMPLETE = "complete";
        /// <summary>
        /// - Animation fade in start.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public const string FADE_IN = "fadeIn";
        /// <summary>
        /// - Animation fade in complete.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public const string FADE_IN_COMPLETE = "fadeInComplete";
        /// <summary>
        /// - Animation fade out start.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public const string FADE_OUT = "fadeOut";
        /// <summary>
        /// - Animation fade out complete.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public const string FADE_OUT_COMPLETE = "fadeOutComplete";
        /// <summary>
        /// - Animation frame event.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public const string FRAME_EVENT = "frameEvent";
        /// <summary>
        /// - Animation frame sound event.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public const string SOUND_EVENT = "soundEvent";

        /// <internal/>
        /// <private/>
        /// <summary>
        /// - The armature that dispatch the event.
        /// </summary>
        /// <see cref="DBKernel.Armature"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// - The custom data.
        /// </summary>
        /// <see cref="DBKernel.CustomData"/>
        /// <private/>
        /// <version>DragonBones 5.0</version>
        /// <language>en_US</language>
        public static void ActionDataToInstance(ActionData data, EventObject instance, Armature armature)
        {
            if (data.type == ActionType.Play)
            {
                instance.type = EventObject.FRAME_EVENT;
            }
            else
            {
                instance.type = data.type == ActionType.Frame ? EventObject.FRAME_EVENT : EventObject.SOUND_EVENT;
            }

            instance.name = data.name;
            instance.armature = armature;
            instance.actionData = data;
            instance.data = data.data;

            if (data.bone != null)
            {
                instance.bone = armature.Structure.GetBone(data.bone.Name);
            }

            if (data.slot != null)
            {
                instance.slot = armature.Structure.GetSlot(data.slot.Name);
            }
        }

        /// <summary>
        /// - If is a frame event, the value is used to describe the time that the event was in the animation timeline. (In seconds)
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public float time;
        /// <summary>
        /// - The event type。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public string type;
        /// <summary>
        /// - The event name. (The frame event name or the frame sound name)
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public string name;
        public Armature armature;
        /// <summary>
        /// - The bone that dispatch the event.
        /// </summary>
        /// <see cref="DBKernel.Bone"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public Bone bone;
        /// <summary>
        /// - The slot that dispatch the event.
        /// </summary>
        /// <see cref="DBKernel.Slot"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        public Slot slot;
        /// <summary>
        /// - The animation state that dispatch the event.
        /// </summary>
        /// <see cref="DBKernel.AnimationState"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        public AnimationState animationState;
        /// <private/>
        public ActionData actionData;
        public UserData data;

        public override void OnReleased()
        {
            this.time = 0.0f;
            this.type = string.Empty;
            this.name = string.Empty;
            this.armature = null;
            this.bone = null;
            this.slot = null;
            this.animationState = null;
            this.actionData = null;
            this.data = null;
        }
    }
}