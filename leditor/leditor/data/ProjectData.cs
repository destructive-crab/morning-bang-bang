using System.Numerics;
using leditor.root.export;
using Newtonsoft.Json;
using SFML.Graphics;

namespace leditor.root;

public sealed class ProjectData
{
    public TextureData[] Textures => textures.ToArray();
    public TileData[] Tiles => tiles.ToArray();
    public TilemapData[] Tilemaps => tilemaps.ToArray();
    public UnitData[] Units => units.ToArray();

    public readonly int TILE_HEIGHT = 512;

    public readonly int TILE_WIDTH = 512;

    private readonly List<TextureData> textures = new();

    private readonly List<TileData> tiles = new();

    private readonly List<TilemapData> tilemaps = new();

    private readonly List<UnitData> units = new();

    private readonly Dictionary<string, TextureData> texturesMap = new();

    private readonly Dictionary<string, TileData> tilesMap = new();

    private readonly Dictionary<string, TilemapData> tilemapsMap = new();

    private readonly Dictionary<string, UnitData> unitsMap = new();

    public event Action<object, object> OnEdited; 
    
    public string Export()
    {
        ProjectDataExportRepresentation representation = new(Tilemaps, Tiles, Units, Textures);
        
        string output = JsonConvert.SerializeObject(representation);

        return output;
    }
    public static ProjectData Import(string text)
    {
        ProjectDataExportRepresentation? representation = JsonConvert.DeserializeObject<ProjectDataExportRepresentation>(text);
        ProjectData projectData = new();

        if (representation != null)
        {
            Console.WriteLine("STARTING IMPORT");
            foreach (TextureData texture in representation.Textures)
            {
                projectData.AddTexture(texture);
                Console.WriteLine($"IMPORT TEXTURE {texture.textureID}");
            }

            foreach (TileData tile in representation.Tiles)
            {
                projectData.AddTile(tile);
                Console.WriteLine($"IMPORT TILE {tile.id}");
            }
            
            foreach (TilemapData tilemap in representation.Tilemaps)
            {
                projectData.AddMap(tilemap);
                tilemap.RefreshData();
                Console.WriteLine($"IMPORT MAP {tilemap.id}");
            }
            
            foreach (UnitData unit in representation.Units)
            {
                projectData.AddUnit(unit);
                Console.WriteLine($"IMPORT UNIT {unit.UnitID}");
            }
        }
        else
        {
            LGR.PERR("CAN NOT IMPORT PROJECT");
            Console.WriteLine("CAN NOT IMPORT PROJECT");
        }

        return projectData;
    }

    public void AddTile(string tileID, TextureData data)
    {
        TileData newTile = new TileData(tileID);
        newTile.texture_id = data.textureID;
        AddTile(newTile);
    }

    private void AddTile(TileData tileData)
    {
        tiles.Add(tileData);
        tilesMap.Add(tileData.id, tileData);
        
        OnEdited?.Invoke(Tiles, tileData);
    }

    public void AddMap(string mapID, KeyValuePair<Vector2, string>[] tiles)
    {
        TilemapData tilemapData = new TilemapData(mapID, tiles);
        
        AddMap(tilemapData);
    }

    public void AddMap(TilemapData tilemapData)
    {
        tilemaps.Add(tilemapData);
        tilemapsMap.Add(tilemapData.id, tilemapData);
        
        OnEdited?.Invoke(Tilemaps, tilemapData);
    }

    public void AddUnit(string unitID, string mapID, string overrideID)
    {
        UnitData unit = new UnitData(unitID, mapID, overrideID);
        AddUnit(unit);
    }

    public void AddUnit(UnitData unitData)
    {
        units.Add(unitData);
        unitsMap.Add(unitData.UnitID, unitData);
        
        OnEdited?.Invoke(Units, unitData);
    }

    public TextureData[] CreateTilesFromTileset(string generalID, string tilesetPath)
    {
        List<TextureData> result = new();
        Image img = AssetsStorage.GetImageAtPath(tilesetPath);

        int setW = (int)(img.Size.X / TILE_WIDTH);
        int setH = (int)(img.Size.Y / TILE_HEIGHT);

        int idIndex = 1;
        for (int w = 0; w < setW; w++)
        {
            for (int h = 0; h < setH; h++)
            {
                Rect rec = new Rect(TILE_WIDTH * w, TILE_HEIGHT * h, TILE_WIDTH, TILE_HEIGHT);

                Image croppedGuiImage = new Image(img);
                TextureData textureData = AddTexture(generalID + $"_{idIndex}", tilesetPath, rec);

                result.Add(textureData);
                AddTile(generalID + $"_{idIndex}", textureData);
                
                idIndex++;
            }
        }
        
        return result.ToArray();
    }
    
    public TextureData AddTexture(string id, string texturePath)
    {
        TextureData newTexture = new TextureData(id, texturePath);
        
        AddTexture(newTexture);
        
        return newTexture;
    }
    
    public TextureData AddTexture(string id, string texturePath, Rect rectangle)
    {
        TextureData newTexture = new TextureData(id, texturePath, rectangle);

        AddTexture(newTexture);
        
        return newTexture;
    }
    
    public TextureData GetTexture(string id)
    {
        return texturesMap[id];
    }

    public TextureData GetTextureFromTileID(string tileID)
    {
        return GetTexture(GetTile(tileID).texture_id);
    }

    public TileData GetTile(string id)
    {
        return tilesMap[id];
    }

    public TilemapData GetMap(string id)
    {
        return tilemapsMap[id];
    }

    public UnitData GetUnit(string id)
    {
        return unitsMap[id];
    }

    public void AddTexture(TextureData textureData)
    {
        textures.Add(textureData);
        texturesMap.Add(textureData.textureID, textureData);
        
        OnEdited?.Invoke(Textures, textureData);
    }
}