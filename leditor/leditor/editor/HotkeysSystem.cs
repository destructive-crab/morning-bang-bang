using SFML.Window;

namespace leditor.root;

public sealed class HotkeysSystem 
{
    public class Hotkey
    {
        public Keyboard.Key Key;
        public Action Callback;

        public Hotkey(Keyboard.Key key, Action callback)
        {
            Key = key;
            Callback = callback;
        }
    }

    private readonly Dictionary<Keyboard.Key, Hotkey> map = new();
    
    public void Push(Keyboard.Key key, Action callback)
    {
        map.Add(key, new Hotkey(key, callback));
    }

    public void Update()
    {
        foreach (KeyValuePair<Keyboard.Key, Hotkey> hotkey in map)
        {
 //           if (Raylib.IsKeyPressed(hotkey.Key))
 //           {
 //               hotkey.Value.Callback.Invoke();
 //           }
        }
    }
}