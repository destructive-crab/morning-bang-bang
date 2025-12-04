namespace leditor.UI;

public class UIVar<T>
{
    private T _value;

    public UIVar(T value, Action<T> onSet = null)
    {
        _value = value;
        OnSet += onSet;
    }

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

    public void ClearCallbacks()
    {
        OnSet = null;
    }
}