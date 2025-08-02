namespace DragonBones
{
    public delegate void ListenerDelegate<T>(string type, T eventObject);
    /// <summary>
    /// - The event dispatcher interface.
    /// Dragonbones event dispatch usually relies on docking engine to implement, which defines the event method to be implemented when docking the engine.
    /// </summary>
    /// <version>DragonBones 4.5</version>
    /// <language>en_US</language>

    public interface IEventDispatcher<T>
    {
        /// <summary>
        /// - Checks whether the object has any listeners registered for a specific type of event。
        /// </summary>
        /// <param name="type">- Event type.</param>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        bool HasDBEventListener(string type);
        /// <summary>
        /// - Dispatches an event into the event flow.
        /// </summary>
        /// <param name="type">- Event type.</param>
        /// <param name="eventObject">- Event object.</param>
        /// <see cref="DBKernel.EventObject"/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        void DispatchDBEvent(string type, T eventObject);
        /// <summary>
        /// - Add an event listener object so that the listener receives notification of an event.
        /// </summary>
        /// <param name="type">- Event type.</param>
        /// <param name="listener">- Event listener.</param>
        /// <param name="thisObject">- The listener function's "this".</param>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        void AddDBEventListener(string type, ListenerDelegate<T> listener);
        /// <summary>
        /// - Removes a listener from the object.
        /// </summary>
        /// <param name="type">- Event type.</param>
        /// <param name="listener">- Event listener.</param>
        /// <param name="thisObject">- The listener function's "this".</param>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        void RemoveDBEventListener(string type, ListenerDelegate<T> listener);
    }
}
