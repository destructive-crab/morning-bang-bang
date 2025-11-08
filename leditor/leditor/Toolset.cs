using System.Numerics;
using deGUISpace;
using leditor.root.deGUILeditor;

namespace leditor.root;

public sealed class Toolset
{
    private Leditor leditor;
    
    public Tool PreviousTool { get; private set; }
    public Tool CurrentTool { get; private set; }

    public readonly Tool[] All;
    
    public Toolset(Leditor leditor)
    {
        this.leditor = leditor;
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
    }

    public void SelectTool(Tool tool)
    {
        PreviousTool?.DisableGUIMenu(leditor.project);
        CurrentTool = tool;
        tool.EnableGUIMenu(leditor.project);
    }
}

internal sealed class PaintTool : Tool
{
    public string selectedTile;
    public override void OnClick(Vector2 position, GridBuffer buffer)
    {
        buffer.SetTile(position, selectedTile);
    }

    public override string Name => nameof(PaintTool);
}

internal sealed class Eraser : Tool
{
    public override void OnClick(Vector2 position, GridBuffer buffer)
    {
        buffer.SetTile(position, null);
    }

    public override string Name => nameof(Eraser);
}

public abstract class Tool
{
    public virtual void EnableGUIMenu(ProjectData project) {}
    public virtual void DisableGUIMenu(ProjectData project) {}
    public abstract void OnClick(Vector2 position, GridBuffer buffer);
    public abstract string Name { get; }
}
