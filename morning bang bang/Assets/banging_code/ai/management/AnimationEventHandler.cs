using System;
using System.Collections.Generic;
using UnityEngine;

namespace banging_code.ai.management
{
    public class AnimationEventHandler : MonoBehaviour
    {
        public const string ATTACK = "Attack";
        
        public Dictionary<string, List<Action>> subscribers = new();

        public void Subscribe(string eventName, Action callback)
        {
            subscribers.TryAdd(eventName, new List<Action>());
            subscribers[eventName].Add(callback);
        }
        
        public void ThrowEvent(string eventName)
        {
            if (subscribers.TryGetValue(eventName, out List<Action> callbacks))
            {
                for(int i = 0; i < callbacks.Count; i++)
                {
                    if (callbacks[i] == null)
                    {
                        callbacks.RemoveAt(i);
                        i--;
                        continue;
                    }
                    
                    callbacks[i].Invoke();
                }
            }
        }
    }
}