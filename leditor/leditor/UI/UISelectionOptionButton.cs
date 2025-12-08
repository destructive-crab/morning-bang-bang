namespace leditor.UI;

public sealed class UISelectionOptionButton : UIButton
{
    private bool isSelected;

    public event Action<UISelectionOptionButton> SelectionChange;
    public bool IsSelected
    {
        get
        {
            return isSelected;
        }
        set
        {
            isSelected = value;
            
            if (isSelected)
            {
                base.ApplyStyle(Host.Style.PressedButton);
            }
            else
            {
                base.ApplyStyle(Host.Style.NormalButton);
            }
            
            SelectionChange?.Invoke(this);
        }
    }

    public UISelectionOptionButton(UIHost host, string text, Action? action = null, bool startState = false) 
        : base(host, text, action)
    {
        ApplyStyle(host.Style.NormalButton);
        IsSelected = startState;
    }

    protected override void OnReleased()
    {
        base.OnReleased();
        IsSelected = !IsSelected;
    }

    protected override void ApplyStyle(ButtonStateStyle style)
    {
        if (IsSelected)
        {
            base.ApplyStyle(appliedStyle);
        }
        else
        {
            base.ApplyStyle(style);
        }
    }
}