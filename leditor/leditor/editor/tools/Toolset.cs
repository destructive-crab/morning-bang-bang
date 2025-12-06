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
    private ProjectData attachedTo;

    public Toolset(ProjectData projectData)
    {
        All = new[] { (Tool)new Eraser(), new PaintTool() };
        attachedTo = projectData;
        attachedTo.OnEdited += (_,_) => CurrentTool?.ProjectDataChanged();
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
    public string SelectedLayer { get; private set; }

    private SplitBox guiRoot;
    private UISelectionList layerOptions;
    private AxisBox tiles;

    public PaintTool()
    {
        UISelectionList layersChooser = UTLS.BuildLayersList(true, (l) => SelectedLayer = l);

        layerOptions = layersChooser;
        
    }

    public void SelectTile(string id)
    {
        SelectedTile = id;
        string[] allowed = App.LeditorInstance.ProjectEnvironment.GetTile(id).AllowLayers;
        
        if (allowed.Contains(SelectedLayer))
        {
            return;
        }

        SelectedLayer = allowed[0];
    }

    public override Texture GetIcon()
    {
        return RenderCacher.GetTexture("assets\\paint.png");
    }

    public override AUIElement BuildUI(UIHost host)
    {
        tiles = new AxisBox(host, UIAxis.Horizontal);
        StackBox layersContainer = new StackBox(host, [new UIRect(host), layerOptions]);

        AddTilesToUI();

        guiRoot = new SplitBox(host, UIAxis.Horizontal, layersContainer, tiles);
        return guiRoot;
    }

    private void AddTilesToUI()
    {
        foreach (TileData tile in App.LeditorInstance.Project.Tiles)
        {
            TextureData textureData = App.LeditorInstance.ProjectEnvironment.GetTexture(tile.TextureID);
            Texture tileTexture = RenderCacher.GetTexture(textureData.PathToTexture);
            
            tiles.AddChild(new UIImageButton(App.UIHost, tileTexture, textureData.TextureRect, tile.ID, new Vector2f(60f/textureData.Width, 60f/textureData.Height), () => SelectTile(tile.ID)));
        }
    }

    public override void ProjectDataChanged()
    {
        tiles.RemoveAllChildren();
        AddTilesToUI();
    }

    public override void OnClick(Vector2 gridCell, GridBuffer buffer)
    {
        buffer.SetTileAt(SelectedLayer, gridCell, SelectedTile);
    }

    public override string Name => nameof(PaintTool);
}

internal sealed class Eraser : Tool
{
    public string SelectedLayer { get; private set; }
    public override Texture GetIcon() => RenderCacher.GetTexture("assets\\eraser.png");
    public override AUIElement BuildUI(UIHost host)
    {
        UISelectionList layersChooser = UTLS.BuildLayersList(true, (l) => SelectedLayer = l);
        return layersChooser;
    }

    public override void ProjectDataChanged()
    {
        
    }

    public override void OnClick(Vector2 gridCell, GridBuffer buffer)
    {
        buffer.SetTileAt(SelectedLayer, gridCell, null);
    }

    public override string Name => nameof(Eraser);
}

public abstract class Tool
{
    public abstract Texture GetIcon();
    public abstract AUIElement BuildUI(UIHost host);
    public abstract void ProjectDataChanged();
    public abstract void OnClick(Vector2 gridCell, GridBuffer buffer);
    public abstract string Name { get; }
}
