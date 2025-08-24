namespace DragonBones
{
    /// <summary>
    /// - Play animation interface. (Both Armature and Wordclock implement the interface)
    /// Any instance that implements the interface can be added to the Worldclock instance and advance time by Worldclock instance uniformly.
    /// </summary>
    /// <see cref="WorldClock"/>
    /// <see cref="Armature"/>
    /// <version>DragonBones 3.0</version>
    /// <language>en_US</language>
    public interface IAnimatable
    {
        /// <summary>
        /// - Advance time.
        /// </summary>
        /// <param name="passedTime">- Passed time. (In seconds)</param>
        /// <version>DragonBones 3.0</version>
        /// <language>en_US</language>
        void AdvanceTime(float passedTime);
    }
}
