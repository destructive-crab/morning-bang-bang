namespace DragonBones
{
    /// <summary>
    /// - The armature proxy interface, the docking engine needs to implement it concretely.
    /// </summary>
    /// <see cref="DragonBones.Armature"/>
    /// <version>DragonBones 5.0</version>
    /// <language>en_US</language>
    public interface IArmatureProxy : IEventDispatcher<EventObject>
    {
        /// <internal/>
        /// <private/>
        void DBInit(Armature armature);
        /// <internal/>
        /// <private/>
        void DBClear();
        /// <internal/>
        /// <private/>
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

        /// <summary>
        /// - 释放该实例和骨架。 （骨架会回收到对象池）
        /// </summary>
        /// <example>
        /// TypeScript 风格，仅供参考。
        /// <pre>
        ///     removeChild(armatureDisplay);
        ///     armatureDisplay.dispose();
        /// </pre>
        /// </example>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        void Dispose(bool disposeProxy);
         /// <summary>
         /// - The armature.
         /// </summary>
         /// <version>DragonBones 4.5</version>
         /// <language>en_US</language>
         Armature Armature { get; }
         /// <summary>
         /// - The animation player.
         /// </summary>
         /// <version>DragonBones 3.0</version>
         /// <language>en_US</language>>
         Animation Animation { get; }
    }
}
