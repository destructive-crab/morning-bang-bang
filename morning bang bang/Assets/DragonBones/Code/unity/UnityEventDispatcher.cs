using System.Collections.Generic;
using UnityEngine;

namespace DragonBones
{
    [DisallowMultipleComponent]
    public class UnityEventDispatcher : MonoBehaviour, IEventDispatcher<EventObject>
    {
        private readonly Dictionary<string, ListenerDelegate<EventObject>> _listeners = new();
        
        public void AddDBEventListener(string type, ListenerDelegate<EventObject> listener)
        {
            if (_listeners.ContainsKey(type))
            {
                var delegates = _listeners[type].GetInvocationList();
                for (int i = 0, l = delegates.Length; i < l; ++i)
                {
                    if (listener == delegates[i] as ListenerDelegate<EventObject>)
                    {
                        return;
                    }
                }

                _listeners[type] += listener;
            }
            else
            {
                _listeners.Add(type, listener);
            }

        }

        public void DispatchDBEvent(string type, EventObject eventObject)
        {
            if (!_listeners.ContainsKey(type))
            {
                return;
            }
            else
            {
                _listeners[type](type, eventObject);
            }
        }

        public bool HasDBEventListener(string type)
        {
            return _listeners.ContainsKey(type);
        }

        public void RemoveDBEventListener(string type, ListenerDelegate<EventObject> listener)
        {
            if (!_listeners.ContainsKey(type))
            {
                return;
            }

            var delegates = _listeners[type].GetInvocationList();
            for (int i = 0, l = delegates.Length; i < l; ++i)
            {
                if (listener == delegates[i] as ListenerDelegate<EventObject>)
                {
                    _listeners[type] -= listener;
                    break;
                }
            }

            if (_listeners[type] == null)
            {
                _listeners.Remove(type);
            }

        }
    }
}