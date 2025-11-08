using System.Numerics;
using deGUISpace;
using leditor.root.deGUILeditor;

namespace leditor.root;

public sealed class Toolset
{
    public Tool PreviousTool { get; private set; }
    public Tool CurrentTool { get; private set; }

    public readonly Tool[] All;
    
    public Toolset()
    {
        All = new[] { (Tool)new Eraser(), new PaintTool() };
    }

    public void BuildGUI()
    {
        GUIGroup group = new GUIGroup(new RectGUIArea(Anchor.LeftTop, 0, 50, 100, -1));

        for (var i = 0; i < All.Length; i++)
        {
            Tool tool = All[i];
            ToolButton toolButton = new (tool.Name, new RectGUIArea(Anchor.CenterTop, 0, 10 + 40 * i, -1, 30));
            toolButton.ApplyTool(tool, this);
            
            group.AddChild(toolButton);
        }
        
        deGUI.PushGUIElement(group);
        
        foreach (Tool tool in All)
        {
            tool.BuildGUI();
            tool.DisableGUIMenu();
        }
    }

    public void SelectTool(Tool tool)
    {
        CurrentTool?.DisableGUIMenu();
        PreviousTool = CurrentTool;
        CurrentTool = tool;
        tool.EnableGUIMenu();
    }
}

public sealed class PaintTool : Tool
{
    public string SelectedTile { get; private set; }

    private GUIGroup group;

    public void SelectTile(string id)
    {
        SelectedTile = id;
    }
    
    public override void BuildGUI()
    {
        group = new GUIGroup(new RectGUIArea(Anchor.CenterBottom, 0, 0, -1, 30));
        
        for (var i = 0; i < App.LeditorInstance.project.Tiles.Length; i++)
        {
            TileData tile = App.LeditorInstance.project.Tiles[i];

            int x = 100;
            int y = 30;

            var button = new TileButton(tile.id, new RectGUIArea(Anchor.LeftBottom, x * i, 0, x, y));
            button.ApplyTool(tile.id, this);
            group.AddChild(button);
        }
        
        deGUI.PushGUIElement(group);
    }

    public override void EnableGUIMenu()
    {
        group.Show();
    }

    public override void DisableGUIMenu()
    {
        group.Hide();
    }

    public override void OnClick(Vector2 gridCell, GridBuffer buffer)
    {
        buffer.SetTile(gridCell, SelectedTile);
    }

    public override string Name => nameof(PaintTool);
}

internal sealed class Eraser : Tool
{
    public override void OnClick(Vector2 gridCell, GridBuffer buffer)
    {
        buffer.SetTile(gridCell, null);
    }

    public override string Name => nameof(Eraser);
}

public abstract class Tool
{
    public virtual void BuildGUI() {}
    public virtual void EnableGUIMenu() {}
    public virtual void DisableGUIMenu() {}
    public abstract void OnClick(Vector2 gridCell, GridBuffer buffer);
    public abstract string Name { get; }
}
