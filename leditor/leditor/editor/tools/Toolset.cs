using System.Numerics;
using leditor.UI;
using SFML.Graphics;
using SFML.System;

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

    public void SelectTool(Tool tool)
    {
        PreviousTool = CurrentTool;
        CurrentTool = tool;
    }
}

public sealed class PaintTool : Tool
{
    public string SelectedTile { get; private set; }

    public void SelectTile(string id)
    {
        SelectedTile = id;
    }

    public override Texture GetIcon()
    {
        return RenderCacher.GetTexture("assets\\paint.png");
    }

    public override AUIElement BuildGUI(UIHost host)
    {
        AxisBox axisBox = new(host, UIAxis.Horizontal);
        
        foreach (TileData tile in App.LeditorInstance.Project.Tiles)
        {
            TextureData textureData = App.LeditorInstance.Project.GetTexture(tile.TextureID);
            Texture tileTexture = RenderCacher.GetTexture(textureData.PathToTexture);
            
            axisBox.AddChild(new UIImageButton(host, tileTexture, textureData.rectangle, tile.ID, new Vector2f(60f/textureData.Width, 60f/textureData.Height), () => SelectTile(tile.ID)));
        }

        return axisBox;
    }
    public override void OnClick(Vector2 gridCell, GridBuffer buffer)
    {
        buffer.SetTile(gridCell, SelectedTile);
    }

    public override string Name => nameof(PaintTool);
}

internal sealed class Eraser : Tool
{
    public override Texture GetIcon() => RenderCacher.GetTexture("assets\\eraser.png");
    public override AUIElement BuildGUI(UIHost host)
    {
        return new UIRect(host, Color.Transparent);
    }

    public override void OnClick(Vector2 gridCell, GridBuffer buffer)
    {
        buffer.SetTile(gridCell, null);
    }

    public override string Name => nameof(Eraser);
}

public abstract class Tool
{
    public abstract Texture GetIcon();
    public abstract AUIElement BuildGUI(UIHost host); 
    public abstract void OnClick(Vector2 gridCell, GridBuffer buffer);
    public abstract string Name { get; }
}
