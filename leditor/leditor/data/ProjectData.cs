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
    public MapData[] Tilemaps => tilemaps.ToArray();
    public UnitData[] Units => units.ToArray();

    private readonly List<TextureData> textures = new();
    private readonly List<TileData> tiles = new();
    private readonly List<MapData> tilemaps = new();
    private readonly List<UnitData> units = new();

    private readonly Dictionary<string, TextureData> texturesMap = new();
    private readonly Dictionary<string, TileData> tilesMap = new();
    private readonly Dictionary<string, MapData> tilemapsMap = new();
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
            
            foreach (MapData tilemap in representation.Tilemaps)
            {
                tilemap.ValidateExternalDataChange();
                projectData.AddMap(tilemap);
                
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
    
    public TextureData? GetTexture(string id)
    {
        if (!texturesMap.ContainsKey(id))
        {
            return null;
        }

        return texturesMap[id];
    }
    public TileData? GetTile(string id)
    {
        if (!tilesMap.ContainsKey(id))
        {
            return null;
        }
        return tilesMap[id];
    }
    public MapData? GetMap(string id)
    {
        if (!tilemapsMap.ContainsKey(id))
        {
            return null;
        }
        return tilemapsMap[id];
    }
    public UnitData? GetUnit(string id)
    {
        if (!unitsMap.ContainsKey(id))
        {
            return null;
        }
        return unitsMap[id];
    }
    
    public void AddTexture(TextureData textureData)
    {
        if(texturesMap.ContainsKey(textureData.ID)) return;
        
        textures.Add(textureData);
        texturesMap.Add(textureData.ID, textureData);
        
        OnEdited?.Invoke(Textures, textureData);
    }
    public void AddTile(TileData tileData)
    {
        if(tilesMap.ContainsKey(tileData.ID)) return;
        //validate tileData
        
        tiles.Add(tileData);
        tilesMap.Add(tileData.ID, tileData);
        
        OnEdited?.Invoke(Tiles, tileData);
    }
    public void AddMap(MapData mapData)
    {
        if (tilemapsMap.ContainsKey(mapData.ID)) return;
        
        tilemaps.Add(mapData);
        tilemapsMap.Add(mapData.ID, mapData);
        
        OnEdited?.Invoke(Tilemaps, mapData);
    }
    public void AddUnit(UnitData unitData)
    {
        if (unitsMap.ContainsKey(unitData.ID)) return;
        
        units.Add(unitData);
        unitsMap.Add(unitData.ID, unitData);
        
        OnEdited?.Invoke(Units, unitData);
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
}