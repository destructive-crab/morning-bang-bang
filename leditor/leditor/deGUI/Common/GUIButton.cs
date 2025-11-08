using System;
using Raylib_cs;

namespace deGUISpace;

public class GUIButton : GUIElement, IOnRightClick, IOnLeftClick, IOnPressed, IOnPressReleased, IOnHoverReleased, IOnHover
{
    public Anchor Anchor => GUIArea.Anchor;
    
    //contents
    public string Label = "";
    
    public Texture2D Texture;
    public Color Color = Color.White;
    
    public Color DefaultColor = Color.White;
    public Color PressedColor = Color.Gray;

    //interactions
    private Action RightCallbacks;
    private Action LeftCallbacks;

    public GUIButton()
    {
    }

    public GUIButton(string label, Color defaultColor, Color pressedColor)
    {
        Label = label;
        DefaultColor = defaultColor;
        PressedColor = pressedColor;
    }

    public GUIButton(string label, RectGUIArea guiArea)
    {
        Label = label;
        GUIArea = guiArea;
    }
    
    public GUIButton(Texture2D texture)
    {
        Texture = texture;
    }

    public GUIButton(string label)
    {
        Label = label;
    }

    public GUIButton(string label, Texture2D texture)
    {
        Label = label;
        Texture = texture;
    }

    public GUIButton(string label, Texture2D texture, Action rightCallbacks, Action leftCallbacks, GUIElement parent)
    {
        Label = label;
        Texture = texture;
        RightCallbacks = rightCallbacks;
        LeftCallbacks = leftCallbacks;
        Parent = parent;
    }

    public void ApplyArea(RectGUIArea area)
    {
        GUIArea = area;
    }

    public void AddCallback(Action left, Action right)
    {
        if(left != null) LeftCallbacks += left;
        if(right != null) RightCallbacks += right;
    }

    public GUIElement Parent { get; set; }
    
    public virtual void OnRightClick()
    {
        RightCallbacks?.Invoke();
    }

    public virtual void OnLeftClick()
    {
        LeftCallbacks?.Invoke();
    }

    public virtual void LeftMouseButtonPress()
    {
        if(LeftCallbacks != null)
        {
            Color = PressedColor;
        }
    }

    public virtual void RightMouseButtonPress()
    {
        if(RightCallbacks != null)
        {
            Color = PressedColor;
        }
    }

    public void OnHoverReleased() { }

    public void LeftOnPressReleased()
    {
        Color = DefaultColor;
    }

    public void RightOnPressReleased()
    {
        Color = DefaultColor;
    }

    public void OnHover() { }
}