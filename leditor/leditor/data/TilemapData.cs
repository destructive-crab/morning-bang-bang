using System.Numerics;
using Newtonsoft.Json;

namespace leditor.root;

[JsonObject(MemberSerialization.OptIn)]
public sealed class PlacedTile
{
    [JsonProperty] public int x;
    [JsonProperty] public int y;

    [JsonProperty] public string tile_id;

    public PlacedTile() { }

    public PlacedTile(int x, int y, string tileId)
    {
        this.x = x;
        this.y = y;
        tile_id = tileId;
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class TilemapData
{
    public PlacedTile[] Get => tiles.ToArray();
    
    [JsonProperty] public string id;
    [JsonProperty] private readonly List<PlacedTile> tiles = new();
    
    private readonly Dictionary<Vector2, string> map = new();

    public TilemapData() { }

    public TilemapData(string id)
    {
        this.id = id;
    }
    
    public TilemapData(string id, KeyValuePair<Vector2, string>[] tiles)
    {
        this.id = id;
        RewriteWith(tiles);
    }

    public void RefreshData()
    {
        foreach (PlacedTile tile in tiles)
        {
            map.Add(new Vector2(tile.x, tile.y), id);
        }
    }

    public void RewriteWith(KeyValuePair<Vector2,string>[] tiles)
    {
        map.Clear();
        this.tiles.Clear();
        
        foreach (KeyValuePair<Vector2,string> pair in tiles)
        {
            map.Add(pair.Key, pair.Value);
            this.tiles.Add(new PlacedTile((int)pair.Key.X, (int)pair.Key.Y, pair.Value));
        }
    }
}