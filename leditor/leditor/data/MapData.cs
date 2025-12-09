using System.Numerics;
using Newtonsoft.Json;

namespace leditor.root;

[JsonObject(MemberSerialization.OptIn)]
public sealed class PlacedTile
{
    [JsonProperty] public int X;
    [JsonProperty] public int Y;

    [JsonProperty] public string TileID;

    public PlacedTile() { }

    public PlacedTile(int x, int y, string tileId)
    {
        this.X = x;
        this.Y = y;
        TileID = tileId;
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

    public void RewriteWith(KeyValuePair<Vector2,string>[] tiles)
    {
        Clear();
        foreach (KeyValuePair<Vector2,string> pair in tiles)
        {
            this.tiles.Add(new PlacedTile((int)pair.Key.X, (int)pair.Key.Y, pair.Value));
        }
    }

    public void RewriteWith(PlacedTile[] tiles)
    {
        Clear();
        foreach (PlacedTile tile in tiles)
        {
            this.tiles.Add(new PlacedTile((int)tile.X, (int)tile.Y, tile.TileID));
        }
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class MapData : LEditorDataUnit
{
    public const string FloorID             = "floor";
    public const string FloorOverlayID      = "floor_overlay";
    public const string ObstaclesID         = "obstacles";
    public const string ObstaclesOverlayID  = "obstacles_overlay";

    public static readonly string[] AllLayers = {FloorID, FloorOverlayID, ObstaclesID, ObstaclesOverlayID };

    public readonly MapLayer Floor                   = new(FloorID);
    public readonly MapLayer FloorOverlay            = new(FloorOverlayID);
    public readonly MapLayer Obstacles               = new(ObstaclesID);    
    public readonly MapLayer ObstaclesOverlay        = new(ObstaclesOverlayID);

    public MapData() { }
    public MapData(string id) => ID = id;

    public override bool ValidateExternalDataChange()
    {
        if(!UTLS.ValidString(ID)) return false;
        
        if (Floor.ID != FloorID)                       return false;
        if (FloorOverlay.ID != FloorOverlayID)         return false;
        if (Obstacles.ID != ObstaclesID)               return false;
        if (ObstaclesOverlay.ID != ObstaclesOverlayID) return false;
        
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