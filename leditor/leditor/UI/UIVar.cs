namespace leditor.UI;

public class UIVar<T>(T value)
{
    private T _value = value;
    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            OnSet?.Invoke(_value);
        }
    }

    public event Action<T>? OnSet;
}