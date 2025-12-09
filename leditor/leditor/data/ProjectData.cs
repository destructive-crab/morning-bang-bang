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
    public MapData[] Maps => maps.ToArray();
    public UnitData[] Units => units.ToArray();

    private readonly List<TextureData> textures = new();
    private readonly List<TileData> tiles = new();
    private readonly List<MapData> maps = new();
    private readonly List<UnitData> units = new();

    private readonly Dictionary<string, TextureData> texturesMap = new();
    private readonly Dictionary<string, TileData> tilesMap = new();
    private readonly Dictionary<string, MapData> mapsMap = new();
    private readonly Dictionary<string, UnitData> unitsMap = new();

    public event Action<EditEntry> OnEdited; 
    
    public string Export()
    {
        ProjectDataExportRepresentation representation = new(Maps, Tiles, Units, Textures);
        
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
        if (!mapsMap.ContainsKey(id))
        {
            return null;
        }
        return mapsMap[id];
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
        
        PushProjectEdit(EditEntry.Textures, Textures, textureData);
    }
    public void AddTile(TileData tileData)
    {
        if(tilesMap.ContainsKey(tileData.ID)) return;
        tiles.Add(tileData);
        tilesMap.Add(tileData.ID, tileData);
        
        PushProjectEdit(EditEntry.Tiles, Tiles, tileData);
    }
    public void AddMap(MapData mapData)
    {
        if (mapsMap.ContainsKey(mapData.ID)) return;
        maps.Add(mapData);
        mapsMap.Add(mapData.ID, mapData);
        
        PushProjectEdit(EditEntry.Maps, Maps, mapData);
    }
    public void AddUnit(UnitData unitData)
    {
        if (unitsMap.ContainsKey(unitData.ID)) return;
        units.Add(unitData);
        unitsMap.Add(unitData.ID, unitData);
        
        PushProjectEdit(EditEntry.Units, Units, unitData);
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
    public void AddMaps(MapData[] data)
    {
        foreach (MapData mapData in data)
        {
            AddMap(mapData);
        }
    }

    public void RemoveTexture(TextureData textureData)
    {
        if (!texturesMap.ContainsKey(textureData.ID)) return;
        textures.Remove(textureData);
        texturesMap.Remove(textureData.ID);
        
        PushProjectEdit(EditEntry.Textures, Textures, textureData);
    }
    public void RemoveTile(TileData tileData)
    {
        if (!tilesMap.ContainsKey(tileData.ID)) return;
        tiles.Remove(tileData);
        tilesMap.Remove(tileData.ID);
        
        PushProjectEdit(EditEntry.Tiles, Tiles, tileData);
    }
    public void RemoveMap(MapData mapData)
    {
        if (!mapsMap.ContainsKey(mapData.ID)) return;
        maps.Remove(mapData);
        mapsMap.Remove(mapData.ID);
        
        PushProjectEdit(EditEntry.Maps, Maps, Units);
    }
    public void RemoveUnit(UnitData unitData)
    {
        if (!unitsMap.ContainsKey(unitData.ID)) return;
        units.Remove(unitData);
        unitsMap.Remove(unitData.ID);
        
        PushProjectEdit(EditEntry.Units, Units, unitData);
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
    public void RemoveAllMaps()
    {
        foreach (MapData mapData in Maps)
        {
            RemoveMap(mapData);
        }
    }

    private readonly List<EditEntry> edits = new();
    private readonly List<EditEntry> editsOnFrame = new();

    public struct EditEntry
    {
        //generic ids
        public const string Textures = "texture_edit";
        public const string Tiles    = "tiles_edit";
        public const string Maps     = "maps_edit";
        public const string Units    = "units_edit";
        
        public readonly string ID;
        public readonly object Where;
        public readonly object Who;

        public EditEntry(string id, object where, object who)
        {
            this.ID = id;
            this.Where = where;
            this.Who = who;
        }
    }
    
    public void PushProjectEdit(string optionalID, object where, object who)
    {
        editsOnFrame.Add(new EditEntry(optionalID, where, who));
    }

    public EditEntry[] PullEdits()
    {
        EditEntry[] edits = new EditEntry[editsOnFrame.Count];
        for (var i = 0; i < edits.Length; i++)
        {
            var edit = editsOnFrame[i];
            edits[i] = edit;
            OnEdited?.Invoke(edit);
        }

        editsOnFrame.Clear();
        this.edits.AddRange(edits);
        
        return edits;
    }

}