using System.Numerics;
using Raylib_cs;

namespace leditor.root;

public sealed class GridBuffer
{
    public int BufferWidth  { get; private set; }
    public int BufferHeight { get; private set; }
    
    public int MinX { get; private set; }
    public int MinY { get; private set; }
    public int MaxX { get; private set; }
    public int MaxY { get; private set; }


    public int WorldBufferWidth => BufferWidth * CELL_SIZE;
    public int WorldBufferHeight => BufferHeight * CELL_SIZE;
    
    public int WorldMinX => MinX * CELL_SIZE;
    public int WorldMinY => MinY * CELL_SIZE;
    public int WorldMaxX => MaxX * CELL_SIZE;
    public int WorldMaxY => MaxY * CELL_SIZE;
    
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
            UpdateBufferRect();
            
            return;
        }
        
        if (id == null)
        {
            return;
        }

        map[pos] = id;
        UpdateBufferRect();
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
        UpdateBufferRect();
    }

    public void AddAbove(TilemapData data)
    {
        foreach (KeyValuePair<Vector2,string> pair in data.Get)
        {
            map[pair.Key] = pair.Value;
        }
        UpdateBufferRect();
    }

    public void UpdateBufferRect()
    {
        if (map.Count == 0)
        {
            MinX = 0;
            MinY = 0;
            MaxX = 0;
            MaxY = 0;
            BufferWidth = 0;
            BufferHeight = 0;
        }
        
        int minX = Int32.MaxValue;
        int maxX = Int32.MinValue;
        int minY = Int32.MaxValue;
        int maxY = Int32.MinValue;
        
        foreach (KeyValuePair<Vector2,string> pair in map)
        {
            if (pair.Key.X < minX)
            {
                minX = (int)pair.Key.X;
            }
            else if (pair.Key.X > maxX)
            {
                maxX = (int)pair.Key.X;
            }
            
            if (pair.Key.Y < minY)
            {
                minY = (int)pair.Key.Y;
            }
            else if (pair.Key.Y > maxY)
            {
                maxY = (int)pair.Key.Y;
            }
        }

        MinX = minX;
        MinY = minY;
        MaxX = maxX + 1;
        MaxY = maxY + 1;

        BufferWidth = (int)MathF.Abs(MaxX - MinX);
        BufferHeight = (int)MathF.Abs(MaxY - MinY);
    }
}