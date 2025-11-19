using System;
using System.Collections.Generic;
using System.Numerics;
using leditor.root;
using SFML.Graphics;
using SFML.System;

namespace deGUISpace;

public enum Anchor
{
    Center,
    
    CenterTop,
    CenterBottom,
    RightCenter,
    LeftCenter,
    
    LeftTop,
    LeftBottom,
    RightTop,
    RightBottom,
}

public static class deGUI
{
    public static bool HoveringSomething { get; private set; }
    
    public static int ORIGINAL_WIDTH = 1280;
    public static int ORIGINAL_HEIGHT = 1040;

    public static GUIElement[] Elements => elements.ToArray();
    public const int STRETCH = -1;

    private static readonly List<GUIElement> elements = new();
    private static readonly List<GUIElement> roots = new();
    
    public static float WCF;
    public static float HCF;

    public static float SCALE_COFF;

    public static RectGUIArea BindRect(int x, int y, int width, int height, Anchor anchor)
    {
        return new RectGUIArea(anchor, x, y, width, height);
    }

    public static void PushGUIElement(GUIElement element)
    {
        if (element == null)
        {
            return;
        }

        elements.Add(element);

        if (element.Parent == null)
        {
            MarkAsRoot(element);
        }
    }
    
    public static GUIButton PushButton(Anchor anchor, int xOffset, int yOffset, int xScale, int yScale, string label, Action left, Action right)
    {
        GUIButton button = new GUIButton(label);
        
        RectGUIArea guiArea = BindRect(xOffset, yOffset, xScale, yScale, anchor);
        button.AddCallback(left, right);
        button.ApplyArea(guiArea);
        
        PushGUIElement(button);
        return button;
    }

    public static GUIImage PushImage(Texture texture, Anchor anchor, int xOffset, int yOffset, int width, int height)
    {
        GUIImage image = new GUIImage(new RectGUIArea(anchor, xOffset, yOffset, width, height), texture);
        
        elements.Add(image);

        return image;
    }

    public static void MarkAsRoot(GUIElement element)
    {
        if (element.Parent == null)
        {
            roots.Add(element);
        }
    }

    public static void RemoveFromRoots(GUIElement element)
    {
        if (element.Parent != null)
        {
            roots.Remove(element);
        }
    }
    
    public static void Draw()
    {
        WCF = (float)App.WindowHandler.Width / ORIGINAL_WIDTH;
        HCF = (float)App.WindowHandler.Height / ORIGINAL_HEIGHT;

        if (MathF.Abs(1 - WCF) > MathF.Abs(1 - HCF)) SCALE_COFF = WCF;
        else SCALE_COFF = HCF;

        List<GUIElement> toProcess = new();

        foreach (GUIElement root in roots)
        {
            ProcessGraph(root);
        }

        void ProcessGraph(GUIElement root)
        {
            if (root.Active)
            { 
                if(root.GUIArea != null)
                {
                    toProcess.Add(root);
                }

                foreach (GUIElement child in root.GUIElementChildren)
                {
                    ProcessGraph(child);
                }
            }
        }
        
        ProcessInteractions(toProcess.ToArray());
        
        foreach (GUIElement element in toProcess)
        {
            DrawElement(element);
        }
    }

    private static List<GUIElement> lastFrameHovering = new();
    private static List<GUIElement> lastFramePressed = new();
    public static void ProcessInteractions(GUIElement[] elements)
    {
        Vector2i mousePos = App.InputsHandler.MousePosition;
        List<GUIElement> hovering = new();
        List<GUIElement> unhovering = new();
        
        foreach (GUIElement element in elements)
        {
            if (element.GUIArea.Fit(mousePos))
            {
                hovering.Add(element);

                if (element is IOnHover onHover)
                {
                    onHover.OnHover();
                }
            }
            else
            {
                unhovering.Add(element);
                    
                if (element is IOnHoverReleased onHoverReleased)
                {
                    onHoverReleased.OnHoverReleased();
                }
            }
        }

        if (App.InputsHandler.IsLeftMouseButtonReleased)
        {
            foreach (var element in lastFramePressed)
            {
                if (element is IOnPressReleased onPressReleased)
                {
                    onPressReleased.LeftOnPressReleased();
                }
            }
        }
        if (App.InputsHandler.IsRightMouseButtonPressed)
        {
            foreach (var element in lastFramePressed)
            {
                if (element is IOnPressReleased onPressReleased)
                {
                    onPressReleased.RightOnPressReleased();
                }
            }
        }

        foreach (GUIElement element in unhovering)
        {
            if (lastFramePressed.Contains(element) && element is IOnPressReleased onPressReleased)
            {
                onPressReleased.LeftOnPressReleased();
            }
        }
        
        lastFramePressed.Clear();
        
        if (App.InputsHandler.IsLeftMouseButtonPressed)
        {
            foreach (GUIElement element in hovering)
            {
                if (element is IOnPressed onPressed)
                {
                    onPressed.LeftMouseButtonPress();
                    lastFramePressed.Add(element);
                    break;
                }
            }
        }
        if (App.InputsHandler.IsRightMouseButtonPressed)
        {
            foreach (GUIElement element in hovering)
            {
                if (element is IOnPressed onPressed)
                {
                    onPressed.RightMouseButtonPress();
                    lastFramePressed.Add(element);
                    break;
                }
            }
        }
        
        if (App.InputsHandler.IsLeftMouseButtonReleased)
        {
            foreach (GUIElement element in hovering)
            {
                if (element is IOnLeftClick onLeftClick)
                {
                    onLeftClick.OnLeftClick();
                    break;
                }
            }
        }
        
        if (App.InputsHandler.IsRightMouseButtonReleased)
        {
            foreach (GUIElement element in hovering)
            {
                if (element is IOnRightClick onRightClick)
                {
                    onRightClick.OnRightClick();
                    break;
                }
            }
        }

        HoveringSomething = hovering.Count != 0;
    }

    private static void DrawElement(GUIElement element)
    {
        switch (element)
        {
            case GUIButton button:
                DrawButton(button);
                break;
            case GUIImage image:
                DrawImage(image);
                break;
            case GUIText text:
                break;
            case GUIRectangle rectangle:
                DrawRectangle(rectangle);
                break;
        }
    }

    private static void DrawRectangle(GUIRectangle rectangle)
    {
        Vector2 anchoredPosition = GUIUtil.AnchorPosition(rectangle);

        int x = (int)anchoredPosition.X;
        int y = (int)anchoredPosition.Y;
        int xsc = rectangle.GUIArea.AdaptedWidth;
        int ysc = rectangle.GUIArea.AdaptedHeight;
        int ex = x + xsc;
        int ey = y + ysc;
        
        App.WindowHandler.DrawRectangle(x, y, xsc, ysc, rectangle.Color);
        
        //outline
        int t = rectangle.Outline;

        //top
 //       Raylib.DrawLineEx(new Vector2(x, y), new Vector2(ex, y),rectangle.Outline, rectangle.OutlineColor);
 //       //left
 //       Raylib.DrawLineEx(new Vector2(x, y), new Vector2(x, ey), rectangle.Outline, rectangle.OutlineColor);
 //       //bottom
 //       Raylib.DrawLineEx(new Vector2(ex, ey), new Vector2(x, ey), rectangle.Outline, rectangle.OutlineColor);
 //       //right
 //       Raylib.DrawLineEx(new Vector2(ex, ey), new Vector2(ex, y), rectangle.Outline, rectangle.OutlineColor);
    }

    private static void DrawImage(GUIImage image)
    {
        Vector2 anchoredPosition = GUIUtil.AnchorPosition(image.GUIArea);
        Vector2 scale = new Vector2(GUIUtil.AdaptX(image.GUIArea.AdaptedWidth), GUIUtil.AdaptY(image.GUIArea.AdaptedHeight));
        
        //Raylib.DrawTextureEx(image.Texture, anchoredPosition, 0, scale.X, Color.White);
    }

    private static void DrawButton(GUIButton button)
    {
        Vector2 anchoredPosition = GUIUtil.AnchorPosition(button);
        Vector2 scale = new Vector2(button.GUIArea.AdaptedWidth, button.GUIArea.AdaptedHeight);
        
        App.WindowHandler.DrawRectangle((int)anchoredPosition.X-3, (int)anchoredPosition.Y-3, (int)(scale.X + 6), (int)(scale.Y + 6), Color.Black);
        App.WindowHandler.DrawRectangle((int)anchoredPosition.X, (int)anchoredPosition.Y, (int)scale.X, (int)scale.Y, button.Color);

        Text text = new Text(button.Label, App.GeneralFont);
        text.CharacterSize = 18;
        text.FillColor = Color.Black;
        text.Position = new Vector2f(anchoredPosition.X, anchoredPosition.Y);
        
        App.WindowHandler.Draw(text);
    }

}