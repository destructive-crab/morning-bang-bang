using System.Numerics;
using Newtonsoft.Json;

namespace leditor.root;

[JsonObject(MemberSerialization.OptIn)]
public sealed class PlacedTile
{
    [JsonProperty] public int X;
    [JsonProperty] public int Y;

    [JsonProperty] public TileReference TileReference;

    public PlacedTile() { }

    public PlacedTile(int x, int y, TileReference tileReference)
    {
        this.X = x;
        this.Y = y;
        TileReference = tileReference;
    }
}

[JsonObject(MemberSerialization.OptIn)]
public sealed class MapLayer
{
    public PlacedTile[] Get => tiles.ToArray();
    
    [JsonProperty] public readonly string ID;
    [JsonProperty] private readonly List<PlacedTile> tiles = new();

    public MapLayer() {}
    public MapLayer(string id) => ID = id;

    public void ValidateTiles()
    {
        foreach (PlacedTile tile in Get)
        {
            if (tile == null)
            {
                tiles.Remove(tile);
            }
        }
    }
    
    public void Clear()
    {
        tiles.Clear();
    }

    public void RewriteWith(KeyValuePair<Vector2,TileReference>[] tiles)
    {
        Clear();
        foreach (KeyValuePair<Vector2,TileReference> pair in tiles)
        {
            this.tiles.Add(new PlacedTile((int)pair.Key.X, (int)pair.Key.Y, pair.Value));
        }
    }

    public void RewriteWith(PlacedTile[] tiles)
    {
        Clear();
        foreach (PlacedTile tile in tiles)
        {
            this.tiles.Add(new PlacedTile((int)tile.X, (int)tile.Y, tile.TileReference));
        }
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class MapData : LEditorDataUnit
{
    public readonly MapLayer Floor            = new(LayerID.FloorID);
    public readonly MapLayer FloorOverlay     = new(LayerID.FloorOverlayID);
    public readonly MapLayer Obstacles        = new(LayerID.ObstaclesID);    
    public readonly MapLayer ObstaclesOverlay = new(LayerID.ObstaclesOverlayID);

    public MapData() { }
    public MapData(string id) => ID = id;

    public override bool ValidateExternalDataChange()
    {
        if(!UTLS.ValidString(ID)) return false;
        
        if (Floor.ID            != LayerID.FloorID)            return false;
        if (FloorOverlay.ID     != LayerID.FloorOverlayID)     return false;
        if (Obstacles.ID        != LayerID.ObstaclesID)        return false;
        if (ObstaclesOverlay.ID != LayerID.ObstaclesOverlayID) return false;
        
        Floor           .ValidateTiles();
        FloorOverlay    .ValidateTiles();
        Obstacles       .ValidateTiles();
        ObstaclesOverlay.ValidateTiles();
        
        return true;
    }

    public void Clear()
    {
        Floor           .Clear();
        FloorOverlay    .Clear();
        Obstacles       .Clear();
        ObstaclesOverlay.Clear();
    }

    public override void CopyDataFrom(LEditorDataUnit from)
    {
        ID = from.ID;
        
        if (from is not MapData other) return;
        
        Floor           .RewriteWith(other.Floor.Get);
        FloorOverlay    .RewriteWith(other.FloorOverlay.Get);
        Obstacles       .RewriteWith(other.Obstacles.Get);
        ObstaclesOverlay.RewriteWith(other.ObstaclesOverlay.Get);
    }
}