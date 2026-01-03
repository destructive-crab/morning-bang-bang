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
    
    private readonly ProjectData attachedTo;

    public AUIElement GetUIRoot => guiRoot;
    private SplitBox guiRoot;
    private SingleBox toolGUIPlace;

    public string SelectedLayer { get; private set; }

    public Toolset(ProjectData projectData)
    {
        All = new[] { (Tool)new Eraser(this), new PaintTool(this) };
        
        attachedTo = projectData;
        attachedTo.OnEdited += (_) => CurrentTool?.ProjectDataChanged();

        UISelectionList layerSelector = UTLS.BuildLayersList(true, (s) => SelectedLayer = s);
        toolGUIPlace = new SingleBox(App.UIHost);
        guiRoot = new SplitBox(App.UIHost, UIAxis.Horizontal, layerSelector, toolGUIPlace);
    }

    public void SelectTool(Tool tool)
    {
        PreviousTool = CurrentTool;
        CurrentTool = tool;
        toolGUIPlace.Child = tool.BuildUI();
    }

    public void SelectLayer(string id)
    {
        
    }
}

public sealed class PaintTool : Tool
{
    public string SelectedTile { get; private set; }

    private AxisBox tiles;
    private readonly Texture icon;

    public PaintTool(Toolset toolset) : base(toolset)
    {
        icon = new Texture(EditorAssets.LoadIcon("paint.png"));
    }

    public void SelectTile(string id)
    {
 //       SelectedTile = id;
 //       LayerID[] allowed = App.LeditorInstance.ProjectEnvironment.GetTile(id).AllowLayers;
 //       
 //       if (allowed.Contains(Toolset.SelectedLayer))
 //       {
 //           return;
 //       }
//
 //       Toolset.SelectLayer(allowed[0]);
    }

    public override Texture GetIcon()
    {
        return icon;
    }

    public override AUIElement BuildUI()
    {
        UIHost host = App.UIHost;
        tiles = new AxisBox(host, UIAxis.Horizontal);

        AddTilesToUI();

        return tiles;
    }

    private void AddTilesToUI()
    {
  //      foreach (TileData tile in App.LeditorInstance.Project.Tiles)
  //      {
  //          TextureData textureData = App.LeditorInstance.ProjectEnvironment.GetTexture(tile.TextureReference);
  //          Texture tileTexture = RenderCacher.GetTexture(textureData.PathToTexture);
  //          
  //          tiles.AddChild(new AxisBox(App.UIHost, UIAxis.Vertical, [
  //              new UILabel(App.UIHost, tile.ID),
  //              new UIImageButton(App.UIHost, tileTexture, textureData.TextureRect, new Vector2f(60f/textureData.Width, 60f/textureData.Height), () => SelectTile(tile.ID))])
  //              );
  //      }
    }

    public override void ProjectDataChanged()
    {
        tiles.RemoveAllChildren();
        AddTilesToUI();
    }

    public override void OnClick(Vector2 gridCell, GridBuffer buffer)
    {
    //    buffer.SetTileAt(Toolset.SelectedLayer, gridCell, SelectedTile);
    }

    public override string Name => nameof(PaintTool);
}

internal sealed class Eraser : Tool
{
    public string SelectedLayer { get; private set; }
    private Texture icon;

    public Eraser(Toolset toolset) : base(toolset)
    {
        icon = new Texture(EditorAssets.LoadIcon("eraser.png"));
    }

    public override Texture GetIcon() => icon;

    public override AUIElement BuildUI()
    {
        return null;
    }

    public override void ProjectDataChanged()
    {
        
    }

    public override void OnClick(Vector2 gridCell, GridBuffer buffer)
    {
     //   buffer.SetTileAt(SelectedLayer, gridCell, null);
    }

    public override string Name => nameof(Eraser);
}

public abstract class Tool
{
    protected readonly Toolset Toolset;

    protected Tool(Toolset toolset)
    {
        Toolset = toolset;
    }

    public abstract Texture GetIcon();
    public abstract AUIElement BuildUI();
    public abstract void ProjectDataChanged();
    public abstract void OnClick(Vector2 gridCell, GridBuffer buffer);
    public abstract string Name { get; }
}
