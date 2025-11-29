using System.Drawing;
using System.Numerics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Color = SFML.Graphics.Color;

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
    public string Tag { get; private set; }

    private readonly Dictionary<Vector2, string> map = new();

    public const int CELL_SIZE = 80;
    private Vector2i pointingOnCell;

    public GridBuffer()
    {
        UpdateBufferRect();
    }

    public bool IsCellAvailableForActions(int x, int y) 
        => x >= MinX - 1 && x <= MaxX + 1 && y >= MinY - 1 && y <= MaxY + 1;

    public bool IsCellAvailableForActions(Vector2i vector2I) 
        => IsCellAvailableForActions(vector2I.X, vector2I.Y);

    public void UpdateBufferOutput(ProjectData data)
    {
        ProcessBufferInputs();
        DrawTiles(data);
        DrawGridLayout();
    }
    
    public void DrawTiles(ProjectData project)
    {
        foreach (KeyValuePair<Vector2, string> mapPair in map)
        {
            Vector2f pos = new Vector2f(mapPair.Key.X * CELL_SIZE, mapPair.Key.Y * CELL_SIZE);
            
            TileData tile = project.GetTile(mapPair.Value);
            TextureData texture = project.GetTexture(tile.TextureID);
            
            DrawTile(texture, pos);
        }
    }

    public void DrawGridLayout()
    {
        Console.WriteLine(BufferWidth + " " + BufferHeight);
        Color color = Color.White;
        color.A = 169;

        int sx = (MinX - 1) * GridBuffer.CELL_SIZE;
        int sy = (MinY - 1) * GridBuffer.CELL_SIZE;
        
        int x = sx;
        int y = sy;
        
        int dx = GridBuffer.CELL_SIZE;
        int dy = GridBuffer.CELL_SIZE;

        int ex = (MaxX + 1) * GridBuffer.CELL_SIZE;
        int ey = (MaxY + 1) * GridBuffer.CELL_SIZE;
        
        
        for (; x <= ex; x += dx)
        {
            App.WindowHandler.DrawLine(x, sy, x, ey, color);
        }
        for (; y <= ey; y += dy) 
        {
            App.WindowHandler.DrawLine(sx, y, ex, y, color);
        }

        if(IsCellAvailableForActions(pointingOnCell.X, pointingOnCell.Y))
        App.WindowHandler.DrawRectangle((int)(pointingOnCell.X * GridBuffer.CELL_SIZE), (int)(pointingOnCell.Y * GridBuffer.CELL_SIZE),  GridBuffer.CELL_SIZE, GridBuffer.CELL_SIZE, new Color(255, 255, 255, 50));
    }

    private void ProcessBufferInputs()
    {
        int w = App.WindowHandler.Width;
        int h = App.WindowHandler.Height;

        Rectangle area = new Rectangle(w / 2, h / 2, (int)(w * 0.85f), (int)(h * 0.8f));
        Vector2i mousePos = Mouse.GetPosition();

        //other
        Vector2f worldPos = App.InputsHandler.WorldMousePosition;

        var pointingOnRaw = worldPos / GridBuffer.CELL_SIZE;
        
        App.WindowHandler.DrawLine(w/2, 0, w/2, h, Color.Green);
        App.WindowHandler.DrawLine(0, h/2, w, h/2, Color.Green);
        
        //idk why but x needs to be always floored
        pointingOnCell.X = (int)MathF.Floor(pointingOnRaw.X);
        pointingOnCell.Y = (int)MathF.Floor(pointingOnRaw.Y);

        //zoom
        View view = App.WindowHandler.View;

        if (BufferHeight == 0 || BufferWidth == 0)
        {
            view.Size = new Vector2f(App.WindowHandler.Width, App.WindowHandler.Height);
        }

        App.WindowHandler.Zoom += App.InputsHandler.MouseWheelDelta / -10;
        
        //move and tools
        if (App.InputsHandler.IsRightMouseButtonPressed)
        {
            Vector2f delta = (Vector2f)(prevMousePos - App.InputsHandler.MousePosition);
            
            delta.X *= (view.Size.X / w);
            delta.Y *= (view.Size.Y / h);
            
            view.Move(delta);
        }
        else if(App.InputsHandler.IsLeftMouseButtonDown && IsCellAvailableForActions(pointingOnCell))
        {
            App.LeditorInstance.ProjectEnvironment.Toolset.CurrentTool?.OnClick(new Vector2(pointingOnCell.X, pointingOnCell.Y), this);
        }

 //       if (view.Target.X < buffer.WorldMinX - 300) view.Target.X = buffer.WorldMinX - 300;
 //       if (view.Target.X > buffer.WorldMaxX + 300) view.Target.X = buffer.WorldMaxX + 300;
 //       if (view.Target.Y < buffer.WorldMinY - 300) view.Target.Y = buffer.WorldMinY - 300;
 //       if (view.Target.Y > buffer.WorldMaxY + 300) view.Target.Y = buffer.WorldMaxY + 300;

        prevMousePos = App.InputsHandler.MousePosition;
    }
    private Vector2i prevMousePos;

    public GridBuffer(string tag)
    {
        Tag = tag;
    }

    private static void DrawTile(TextureData textureData, Vector2f pos)
    {
        Texture tex = RenderCacher.GetTexture(textureData.PathToTexture);

        Sprite sprite = new Sprite(tex);
        sprite.TextureRect = textureData.rectangle.ToIntRect();
        sprite.Scale =new Vector2f((float)CELL_SIZE / sprite.TextureRect.Width, (float)CELL_SIZE / sprite.TextureRect.Height);
        sprite.Position = pos;
        
        App.WindowHandler.Draw(sprite);
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
        foreach (var pair in data.Get)
        {
            map[new Vector2(pair.x, pair.y)] = pair.tile_id;
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
            BufferWidth = 1;
            BufferHeight = 1;
            
            return;
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
            if (pair.Key.X > maxX)
            {
                maxX = (int)pair.Key.X;
            }
            
            if (pair.Key.Y < minY)
            {
                minY = (int)pair.Key.Y;
            }
            if (pair.Key.Y > maxY)
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
    
    public void FocusOnBufferCenter()
    {
        App.WindowHandler.View.Center = new Vector2f(
            /* x */ WorldMinX + WorldBufferWidth  / 2f,
            /* y */ WorldMinY + WorldBufferHeight / 2f);

        float xZoom = App.WindowHandler.Width  / (float) (WorldBufferWidth  + 240);
        float yZoom = App.WindowHandler.Height / (float) (WorldBufferHeight + 240);

        float ratio = App.WindowHandler.Width / (float)App.WindowHandler.Height;
            
        if (BufferWidth > BufferHeight)
        {
            App.WindowHandler.ViewWidth = WorldBufferWidth + 340;
            App.WindowHandler.ViewHeight = (int)((WorldBufferWidth + 340) * (1f/ratio));
        }
        else
        {
            App.WindowHandler.ViewWidth = (int)((WorldBufferHeight + 340) * ratio);
            App.WindowHandler.ViewHeight = (WorldBufferHeight + 340);
        }
    }
}