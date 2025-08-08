namespace DragonBones
{
    /// <summary>
    /// - The armature proxy interface, the docking engine needs to implement it concretely.
    /// </summary>
    /// <see cref="Armature"/>
    /// <version>DragonBones 5.0</version>
    /// <language>en_US</language>
    public interface IEngineArmatureDisplay : IEventDispatcher<EventObject>
    {
        void DBInit(Armature armature);
        void DBClear(bool disposeDisplay = false);
        void DBUpdate();
        
        /// <summary>
        /// - Dispose the instance and the Armature instance. (The Armature instance will return to the object pool)
        /// </summary>
        /// <example>
        /// TypeScript style, for reference only.
        /// <pre>
        ///     removeChild(armatureDisplay);
        ///     armatureDisplay.dispose();
        /// </pre>
        /// </example>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        void Dispose(bool disposeProxy);

         Armature Armature { get; }
         AnimationPlayer AnimationPlayer { get; }
    }
}
