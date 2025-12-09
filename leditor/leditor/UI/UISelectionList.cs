using SFML.Graphics;
using SFML.System;

namespace leditor.UI;

public sealed class UISelectionList : AUIBox
{
    public UIOption Current;
    public event Action<UIOption> OnChanged; 
    
    public bool IsSingleSelection = true;
    private readonly List<UIOption> options = new();
    private AxisBox box;

    public UISelectionList(UIHost host, UIOption[] options = null, Vector2f minimalSize = default) 
        : base(host, minimalSize)
    {
        box = new AxisBox(host, UIAxis.Vertical);

        if(options==null) return;
        foreach (UIOption option in options)
        {
            AddChild(option);
        }
    }

    public void AddChild(UIOption option)
    {
        options.Add(option);
        option.SelectionChange += ChangeSelection;
        box.AddChild(option);
        UpdateLayout();
    }

    private void ChangeSelection(UIOption changedOption)
    {
        if (!changedOption.IsSelected || !IsSingleSelection) return;
        
        foreach (UIOption option in options)
        {
            if(option == changedOption) continue;
            option.IsSelected = false;
        }

        Current = changedOption;
        OnChanged?.Invoke(Current);
    }

    public override void RemoveChild(AUIElement child)
    {
        if (child is UIOption uiSelectionOptionButton)
        {
            options.Remove(uiSelectionOptionButton);
        }
    }

    public override IEnumerable<AUIElement> GetChildren()
    {
        return [ box ];
    }
    
    protected override void UpdateMinimalSize() { }
    public override void UpdateLayout() { }

    public override void Draw(RenderTarget target)
    {
        box.Rect = new FloatRect(Rect.Position, new(Rect.Width, box.Rect.Size.Y));
        MinimalSize = box.MinimalSize;
        Rect = box.Rect;
        box.Draw(target);
    }
}