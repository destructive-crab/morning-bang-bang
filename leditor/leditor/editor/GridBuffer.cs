using System.Numerics;
using Raylib_cs;

namespace leditor.root;

public sealed class GridBuffer
{
    public KeyValuePair<Vector2, string>[] Get => map.ToArray();
    private readonly Dictionary<Vector2, string> map = new();
    
    public const int CELL_SIZE = 80;

    public void DrawTiles(ProjectData project)
    {
        foreach (KeyValuePair<Vector2, string> mapPair in map)
        {
            Vector2 pos = mapPair.Key;
            
            TileData tile = project.GetTile(mapPair.Value);
            TextureData texture = project.GetTexture(tile.texture_id);
            
            DrawTile(texture, pos);
        }
    }

    private static void DrawTile(TextureData texture, Vector2 pos)
    {
        Texture2D tex = AssetsStorage.GetTexture(texture);
        
        Raylib.DrawTextureEx(tex, pos * CELL_SIZE, 0, (float)CELL_SIZE/tex.Width, Color.White);
    }

    public void SetTile(Vector2 pos, string id)
    {
        if (id == null && map.ContainsKey(pos))
        {
            map.Remove(pos);
            return;
        }
        else if (id == null)
        {
            return;
        }

        map[pos] = id;
    }
    
    public void LoadFromTilemap(TilemapData tilemap)
    {
        
    }
    
    public void Foreach(Action<Vector2, string> tileID)
    {
        foreach (KeyValuePair<Vector2,string> pair in map)
        {
            tileID.Invoke(pair.Key, pair.Value);
        }
    }

    public void Clear()
    {
        map.Clear();
    }

    public void Add(TilemapData data)
    {
        foreach (KeyValuePair<Vector2,string> pair in data.Get)
        {
            map[pair.Key] = pair.Value;
        }
    }
}