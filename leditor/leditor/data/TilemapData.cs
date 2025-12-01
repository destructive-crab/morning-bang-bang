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
public class TilemapData : LEditorDataUnit
{
    public PlacedTile[] Get => tiles.ToArray();
    
    [JsonProperty] private readonly List<PlacedTile> tiles = new();
    private readonly Dictionary<Vector2, string> map = new();

    public TilemapData() { }
    public TilemapData(string id) => ID = id;

    public TilemapData(string id, KeyValuePair<Vector2, string>[] tiles)
    {
        ID = id;
        RewriteWith(tiles);
    }

    public override bool ValidateExternalDataChange()
    {
        foreach (PlacedTile tile in tiles)
        {
            map.Add(new Vector2(tile.X, tile.Y), ID);
            
            if (tile == null)
            {
                return false;
            }
        }
        return true;
    }

    public void Clear()
    {
        map.Clear();
        tiles.Clear();
    }

    public void RewriteWith(KeyValuePair<Vector2,string>[] tiles)
    {
        Clear();
        foreach (KeyValuePair<Vector2,string> pair in tiles)
        {
            map.Add(pair.Key, pair.Value);
            this.tiles.Add(new PlacedTile((int)pair.Key.X, (int)pair.Key.Y, pair.Value));
        }
    }

    public void RewriteWith(PlacedTile[] tiles)
    {
        Clear();
        foreach (PlacedTile tile in tiles)
        {
            map.Add(new Vector2(tile.X, tile.Y), tile.TileID);
            this.tiles.Add(new PlacedTile((int)tile.X, (int)tile.Y, tile.TileID));
        }
    }

    public override void CopyDataFrom(LEditorDataUnit from)
    {
        ID = from.ID;
        
        if (from is not TilemapData tilemapData) return;
        
        RewriteWith(tilemapData.Get);
    }
}