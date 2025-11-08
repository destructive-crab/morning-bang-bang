using System;
using System.Collections.Generic;
using Raylib_cs;

namespace leditor.root;

public sealed class HotkeysSystem 
{
    public class Hotkey
    {
        public KeyboardKey Key;
        public Action Callback;

        public Hotkey(KeyboardKey key, Action callback)
        {
            Key = key;
            Callback = callback;
        }
    }

    private readonly Dictionary<KeyboardKey, Hotkey> map = new();
    
    public void Push(KeyboardKey key, Action callback)
    {
        map.Add(key, new Hotkey(key, callback));
    }

    public void Update()
    {
        foreach (KeyValuePair<KeyboardKey, Hotkey> hotkey in map)
        {
            if (Raylib.IsKeyPressed(hotkey.Key))
            {
                hotkey.Value.Callback.Invoke();
            }
        }
    }
}