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
    
    public string Tag { get; private set; }

    private readonly Dictionary<string, Dictionary<Vector2, string>> map = new();

    public const int CELL_SIZE = 80;
    private Vector2i pointingOnCell;

    public bool BlockInputs = true;

    public GridBuffer()
    {
        UpdateBufferRect();
    }

    public bool IsCellAvailableForActions(int x, int y) 
        => x >= MinX - 1 && x <= MaxX + 1 && y >= MinY - 1 && y <= MaxY + 1;

    public bool IsCellAvailableForActions(Vector2i vector2I) 
        => IsCellAvailableForActions(vector2I.X, vector2I.Y);

    public void UpdateBufferOutput(ProjectEnvironment projectEnvironment)
    {
        ProcessBufferInputs();
        DrawTiles(projectEnvironment);
        DrawGridLayout();
    }
    
    public void DrawTiles(ProjectEnvironment projectEnvironment)
    {
        foreach (KeyValuePair<string, Dictionary<Vector2, string>> layer in map)
        {
            foreach (KeyValuePair<Vector2, string> tilePair in layer.Value)
            {
                Vector2f pos = new Vector2f(tilePair.Key.X * CELL_SIZE, tilePair.Key.Y * CELL_SIZE);
                
                TileData tile = projectEnvironment.GetTile(tilePair.Value);
                TextureData texture = projectEnvironment.GetTexture(tile.TextureID);
                
                DrawTile(texture, pos, projectEnvironment);
            }   
        }
    }

    public void DrawGridLayout()
    {
        Color color = Color.White;
        color.A = 169;

        int sx = (MinX - 1) * GridBuffer.CELL_SIZE;
        int sy = (MinY - 1) * GridBuffer.CELL_SIZE;
       
        int x = sx;
        int y = sy;
        
        int dx = GridBuffer.CELL_SIZE;
        int dy = GridBuffer.CELL_SIZE;

        int ex = (MaxX + 2) * GridBuffer.CELL_SIZE;
        int ey = (MaxY + 2) * GridBuffer.CELL_SIZE;
        
        for (; x <= ex; x += dx)
        {
            App.WindowHandler.DrawLine(x, sy, x, ey, color);
        }
        for (; y <= ey; y += dy) 
        {
            App.WindowHandler.DrawLine(sx, y, ex, y, color);
        }

        if (!BlockInputs && IsCellAvailableForActions(pointingOnCell.X, pointingOnCell.Y))
        {
            App.WindowHandler.DrawRectangle((int)(pointingOnCell.X * GridBuffer.CELL_SIZE), (int)(pointingOnCell.Y * GridBuffer.CELL_SIZE),  GridBuffer.CELL_SIZE, GridBuffer.CELL_SIZE, new Color(255, 255, 255, 50));
        }
    }

    private void ProcessBufferInputs()
    {
        if (BlockInputs) return;
        int w = App.WindowHandler.Width;
        int h = App.WindowHandler.Height;

        Rectangle area = new Rectangle(w / 2, h / 2, (int)(w * 0.85f), (int)(h * 0.8f));
        Vector2i mousePos = Mouse.GetPosition();

        //other
        Vector2f worldPos = App.InputsHandler.WorldMousePosition;

        var pointingOnRaw = worldPos / GridBuffer.CELL_SIZE;
        
        pointingOnCell.X = (int)MathF.Floor(pointingOnRaw.X);
        pointingOnCell.Y = (int)MathF.Floor(pointingOnRaw.Y);

        //zoom
        View view = App.WindowHandler.View;

        if (BufferHeight == 0 || BufferWidth == 0)
        {
            view.Size = new Vector2f(App.WindowHandler.Width, App.WindowHandler.Height);
        }

        App.WindowHandler.Zoom += App.InputsHandler.MouseWheelDelta / -10;
        App.WindowHandler.Zoom = Math.Clamp(App.WindowHandler.Zoom, 0.1f, 4f);
        
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

        prevMousePos = App.InputsHandler.MousePosition;
    }
    private Vector2i prevMousePos;

    public GridBuffer(string tag)
    {
        Tag = tag;
        Clear();
    }

    private static void DrawTile(TextureData textureData, Vector2f pos, ProjectEnvironment projectEnvironment)
    {
        Texture tex = RenderCacher.GetTexture(textureData.PathToTexture);

        float xScale = CELL_SIZE / (float)projectEnvironment.Project.TILE_WIDTH;
        float yScale = CELL_SIZE / (float)projectEnvironment.Project.TILE_HEIGHT;
        
        Sprite sprite      = new Sprite(tex);
        sprite.TextureRect = textureData.TextureRect.ToIntRect();
        sprite.Scale       = new Vector2f(xScale, yScale);
        sprite.Position    = pos - new Vector2f(textureData.Width * xScale * textureData.PivotX / 100f, textureData.Height * yScale * textureData.PivotY / 100f);
        
        Console.WriteLine(new Vector2f(textureData.Width * xScale * textureData.PivotX / 100f, textureData.Height * yScale * textureData.PivotY / 100f));
        Console.WriteLine(new Vector2f(textureData.Width * xScale * textureData.PivotX / 100f, textureData.Height * yScale * textureData.PivotY / 100f));
        
        App.WindowHandler.Draw(sprite);
    }

    public void SetTileAt(string layerID, Vector2 pos, string id)
    {
        if (!map[layerID].ContainsKey(pos)) return;
        
        if (id == null )
        {
            map[layerID].Remove(pos);
            SortTiles();
            UpdateBufferRect();
            
            return;
        }
        
        if (id == null)
        {
            return;
        }

        map[layerID][pos] = id;
        SortTiles();
        UpdateBufferRect();
    }

    private void SortTiles()
    {
        foreach (KeyValuePair<string, Dictionary<Vector2,string>> layer in map)
        {
            var tiles = layer.Value;
            var tilesList = new List<KeyValuePair<Vector2, string>>(tiles);
            tilesList.Sort((pair1, pair2) =>
            {
                var v1 = pair1.Key;
                var v2 = pair2.Key;
    
                if ((int)v1.Y == (int)v2.Y)
                {
                    if (v1.X < v2.X) return -1;
                    if (v1.X > v2.X) return 1;
                }
                
                if (v1.Y < v2.Y) return -1;
                if (v1.Y > v2.Y) return 1;
    
                return 0;
            });

            map[layer.Key].Clear();
            foreach (KeyValuePair<Vector2,string> pair in tilesList)
            {
                map[layer.Key].Add(pair.Key, pair.Value);
            }    
        }
    }

    public void Clear()
    {
        map.Clear();
        foreach (string layer in MapData.AllLayers)
        {
            map.Add(layer, new Dictionary<Vector2, string>());
        }
        UpdateBufferRect();
    }

    public void AddAbove(MapData data)
    {
        foreach (var pair in data.Floor.Get)
        {
            map[MapData.FloorID][new Vector2(pair.X, pair.Y)] = pair.TileID;
        }
        foreach (var pair in data.FloorOverlay.Get)
        {
            map[MapData.FloorOverlayID][new Vector2(pair.X, pair.Y)] = pair.TileID;
        }
        foreach (var pair in data.Obstacles.Get)
        {
            map[MapData.ObstaclesID][new Vector2(pair.X, pair.Y)] = pair.TileID;
        }
        foreach (var pair in data.ObstaclesOverlay.Get)
        {
            map[MapData.ObstaclesOverlayID][new Vector2(pair.X, pair.Y)] = pair.TileID;
        }
        UpdateBufferRect();
    }

    public void UpdateBufferRect()
    {
        if (map[MapData.FloorID].Count == 0 && map[MapData.ObstaclesID].Count == 0)
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

        foreach (var layer in map)
        {
            foreach (KeyValuePair<Vector2,string> pair in layer.Value)
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
        }

        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;

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