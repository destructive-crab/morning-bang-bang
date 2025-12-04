using SFML.Window;

namespace leditor.root;

public sealed class HotkeysSystem 
{
    public class Hotkey
    {
        public Keyboard.Key[] Key;
        public Action Callback;

        public Hotkey(Keyboard.Key[] key, Action callback)
        {
            Key = key;
            Callback = callback;
        }
    }

    private readonly Dictionary<Keyboard.Key[], Hotkey> map = new();
    
    public void Bind(Action callback, params Keyboard.Key[] key)
    {
        map.Add(key, new Hotkey(key, callback));
    }

    public void Update()
    {
        foreach (KeyValuePair<Keyboard.Key[], Hotkey> hotkey in map)
        {
            foreach (Keyboard.Key key in hotkey.Key)
            {
                if (key == Keyboard.Key.LControl && !App.InputsHandler.IsKeyPressed(key))
                {
                    break;
                }
                if (key != Keyboard.Key.LControl && App.InputsHandler.IsKeyDown(key))
                {
                    hotkey.Value.Callback.Invoke();
                }
            }
        }
    }
}