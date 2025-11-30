using System.Numerics;
using leditor.root.export;
using Newtonsoft.Json;
using SFML.Graphics;

namespace leditor.root;

public sealed class ProjectData
{
    public readonly int TILE_HEIGHT = 512;
    public readonly int TILE_WIDTH = 512;
    
    public TextureData[] Textures => textures.ToArray();
    public TileData[] Tiles => tiles.ToArray();
    public TilemapData[] Tilemaps => tilemaps.ToArray();
    public UnitData[] Units => units.ToArray();

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
            LGR.PM("STARTING IMPORT");
            foreach (TextureData texture in representation.Textures)
            {
                texture.ValidateExternalDataChange();
                projectData.AddTexture(texture);
                
                LGR.PM($"IMPORT TEXTURE {texture.ID}");
            }

            foreach (TileData tile in representation.Tiles)
            {
                tile.ValidateExternalDataChange();
                projectData.AddTile(tile);
                
                LGR.PM($"IMPORT TILE {tile.ID}");
            }
            
            foreach (TilemapData tilemap in representation.Tilemaps)
            {
                tilemap.ValidateExternalDataChange();
                projectData.AddMap(tilemap);
                tilemap.RefreshData();
                LGR.PM($"IMPORT MAP {tilemap.ID}");
            }
            
            foreach (UnitData unit in representation.Units)
            {
                unit.ValidateExternalDataChange();
                projectData.AddUnit(unit);
                LGR.PM($"IMPORT UNIT {unit.ID}");
            }
        }
        else
        {
            LGR.PERR("CAN NOT IMPORT PROJECT");
        }

        LGR.PM("Successfully imported");
        return projectData;
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

    public void AddTexture(TextureData textureData)
    {
        if(texturesMap.ContainsKey(textureData.ID)) return;
        
        textures.Add(textureData);
        texturesMap.Add(textureData.ID, textureData);
        
        OnEdited?.Invoke(Textures, textureData);
    }
    
    public void AddTile(string tileID, TextureData data)
    {
        TileData newTile = new TileData(tileID);
        newTile.TextureID = data.ID;
        AddTile(newTile);
    }
    public void AddTile(TileData tileData)
    {
        if(tilesMap.ContainsKey(tileData.ID)) return;
        //validate tileData
        
        tiles.Add(tileData);
        tilesMap.Add(tileData.ID, tileData);
        
        OnEdited?.Invoke(Tiles, tileData);
    }
    
    public void AddMap(string mapID, KeyValuePair<Vector2, string>[] tiles)
    {
        TilemapData tilemapData = new TilemapData(mapID, tiles);
        
        AddMap(tilemapData);
    }
    public void AddMap(TilemapData tilemapData)
    {
        if (tilemapsMap.ContainsKey(tilemapData.ID)) return;
        
        tilemaps.Add(tilemapData);
        tilemapsMap.Add(tilemapData.ID, tilemapData);
        
        OnEdited?.Invoke(Tilemaps, tilemapData);
    }
    
    public void AddUnit(string unitID, string mapID, string overrideID)
    {
        UnitData unit = new UnitData(unitID, mapID, overrideID);
        AddUnit(unit);
    }
    public void AddUnit(UnitData unitData)
    {
        if (unitsMap.ContainsKey(unitData.ID)) return;
        
        units.Add(unitData);
        unitsMap.Add(unitData.ID, unitData);
        
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

    public TextureData GetTexture(string id)
    {
        if (id == null || !texturesMap.ContainsKey(id)) return null;
        
        return texturesMap[id];
    }

    public TextureData GetTextureFromTileID(string tileID)
    {
        return GetTexture(GetTile(tileID).TextureID);
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

    public void RemoveTexture(TextureData textureData)
    {
        if (!texturesMap.ContainsKey(textureData.ID)) return;
        textures.Remove(textureData);
        texturesMap.Remove(textureData.ID);
    }

    public void RemoveTile(TileData tileData)
    {
        if (!tilesMap.ContainsKey(tileData.ID)) return;
        tiles.Remove(tileData);
        tilesMap.Remove(tileData.ID);
    }

    public void RemoveUnit(UnitData unitData)
    {
        if (!unitsMap.ContainsKey(unitData.ID)) return;
        units.Remove(unitData);
        unitsMap.Remove(unitData.ID);
    }

    public void RemoveAllTextures()
    {
        foreach (TextureData textureData in Textures)
        {
            RemoveTexture(textureData);
        }
    }

    public void RemoveAllTiles()
    {
        foreach (TileData tile in Tiles)
        {
            RemoveTile(tile);
        }
    }

    public void RemoveAllUnits()
    {
        foreach (UnitData unitData in Units)
        {
            RemoveUnit(unitData);
        }
    }

    public void AddTextures(TextureData[] data)
    {
        foreach (TextureData textureData in data)
        {
            AddTexture(textureData);
        }
    }
    
    public void AddTiles(TileData[] data)
    {
        foreach (TileData tileData in data)
        {
            AddTile(tileData);
        }
    }

    public void AddUnits(UnitData[] data)
    {
        foreach (UnitData unitData in data)
        {
            AddUnit(unitData);
        }
    }
}