using System.Numerics;
using ImGuiNET;
using rlImGui_cs;

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
    }
    
    public void DrawToolsetGUI(ProjectData project)
    {
        ImGui.Begin("Tools", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
        {
            ImGui.BeginMainMenuBar();

            foreach (Tool tool in All)
            {
                if (ImGui.Button(tool.Name, new Vector2(60, 40)))
                {
                    PreviousTool = CurrentTool;
                    CurrentTool = tool;
                }
            }
            ImGui.EndMenuBar();

            CurrentTool?.DrawToolMenu(project);
        }
        ImGui.End();
    }
}

internal sealed class PaintTool : Tool
{
    public string selectedTile;

    public override void DrawToolMenu(ProjectData project)
    {
        foreach (TileData tile in project.Tiles)
        {
            if (rlImGui.ImageButtonSize(tile.id, AssetsStorage.GetTexture(project.GetTexture(tile.texture_id)),
                    new Vector2(40, 40)))
            {
                selectedTile = tile.id;
            }
        } 

    }

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
    public virtual void DrawToolMenu(ProjectData project) {}
    public abstract void OnClick(Vector2 position, GridBuffer buffer);
    public abstract string Name { get; }
}
