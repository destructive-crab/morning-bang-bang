using System.Numerics;
using Raylib_cs;

namespace leditor.root;

public class ButtonManager
{
    private List<object> buttons = new();

    public Button Push(Anchor anchor, int xOffset, int yOffset, int xScale, int yScale, string label, Action left, Action right)
    {
        Button button = new Button(anchor, xOffset, yOffset, xScale, yScale, label);
        
        buttons.Add(button);
        AreaManager.RectGUIArea guiArea = deGUI.Areas.BindRect(anchor, button.OffsetX, button.OffsetY, button.ScaleX, button.ScaleY, button.Label);
        button.AddCallback(left, right);
        button.ApplyArea(guiArea);

        return button;
    }
    
    public void PushGroup(ButtonGroup group)
    {
        buttons.Add(group);
    }

    public void DrawButtons()
    {
        foreach (object next in buttons)
        {
            if (next is Button button)
            {
                Vector2 position = GUIUtil.AnchorPosition(button);
                
                DrawButton(position, button);
            }
            else if (next is ButtonGroup group)
            {
                Vector2 position = GUIUtil.AnchorPosition(group.buttons[0]);
                
                foreach (Button buttonInGroup in group.buttons)
                {
                    DrawButton(position, buttonInGroup);

                    position.X += buttonInGroup.ScaleX + group.XSpace;
                }
            }
        }
    }

    private void DrawButton(Vector2 anchoredPosition, Button button)
    {
        Vector2 scale = new Vector2(GUIUtil.AdaptX(button.ScaleX), GUIUtil.AdaptY(button.ScaleY));
        
        Raylib.DrawRectangle((int)anchoredPosition.X-3, (int)anchoredPosition.Y-3, (int)(scale.X + 6), (int)(scale.Y + 6), Color.Black);
        Raylib.DrawRectangle((int)anchoredPosition.X, (int)anchoredPosition.Y, (int)scale.X, (int)scale.Y, button.Color);
        
        int count = button.Label.Length;

        int fontSize = (int)(scale.Y - 10);

        if (Raylib.MeasureText(button.Label, fontSize) >= scale.X)
        {
            fontSize = (int)(scale.X / count);

            if (Raylib.MeasureText(button.Label, fontSize) >= scale.X)
            {
                return;
            }
            
            if (fontSize >= scale.Y)
            {
                fontSize = (int)(scale.Y - 4);
            }           
        }

        int posX = (int)anchoredPosition.X + (int)((scale.X - Raylib.MeasureText(button.Label, fontSize)) / 2);
        int posY = (int)anchoredPosition.Y + (int)(scale.Y - fontSize) / 2;

        Raylib.DrawText(button.Label, posX, posY, fontSize, Color.Black);
    }

    public void ProcessInteractions()
    {
        Vector2 mousePos = Raylib.GetMousePosition();
        
        if (Raylib.IsMouseButtonPressed(MouseButton.Right))
        {
            foreach (Button button in buttons)
            {
                if (button.GUIArea.Fit(mousePos.X, mousePos.Y))
                {
                    button.InvokeRightClick();
                }
            }
        }
        
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            foreach (Button button in buttons)
            {
                if (button.GUIArea.Fit(mousePos.X, mousePos.Y))
                {
                    button.InvokeLeftClick();
                }
            }

        }
    }
}