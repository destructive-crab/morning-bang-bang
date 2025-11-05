using leditor.UI;
using Raylib_cs;

namespace leditor.root;

public sealed class Leditor
{
    private UIEditor _ui = new UIEditor();
    
    public void DoLoop()
    {
        while (!Raylib.WindowShouldClose())
        {
            _ui.Update();
            
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            _ui.Draw();
            Raylib.EndDrawing();
        }
    }
}