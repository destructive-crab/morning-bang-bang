using System;
using SFML.System;

namespace leditor.UI;

public sealed class UIOption : UIButton
{
    private bool isSelected;

    public event Action<UIOption> SelectionChange;
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

    public bool IsLocked;
    
    public UIOption(UIHost host, string text, Action? action = null, bool startState = false) 
        : base(host, text, action)
    {
        ApplyStyle(host.Style.NormalButton);
        IsSelected = startState;
    }
    
    public UIOption(UIHost host, string text, Vector2f minimalSize, Action? action = null, bool startState = false) 
        : base(host, text, minimalSize, action)
    {
        ApplyStyle(host.Style.NormalButton);
        IsSelected = startState;
    }

    protected override void OnReleased()
    {
        if(IsLocked) return;
        
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