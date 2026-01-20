using leditor.root.export;
using Newtonsoft.Json;

namespace leditor.root;

public sealed class ProjectData
{
    public readonly int TILE_HEIGHT = 512;
    public readonly int TILE_WIDTH = 512;
    
    public TextureData[] Textures => TexturesStorage.Data;
    public TileData[]    Tiles    => TilesStorage   .Data;
    public MapData[]     Maps     => MapsStorage    .Data;
    public UnitData[]    Units    => UnitsStorage   .Data;

    public readonly DataStorage<LayerData>   LayersStorage;
    public readonly DataStorage<TextureData> TexturesStorage;
    public readonly DataStorage<TileData>    TilesStorage;
    public readonly DataStorage<MapData>     MapsStorage;
    public readonly DataStorage<UnitData>    UnitsStorage;

    private readonly Dictionary<Type, object> storages = new();

    public event Action<EditEntry> OnEdited; 

    public ProjectData()
    {
        LayersStorage   = new DataStorage<LayerData>  ();
        TexturesStorage = new DataStorage<TextureData>();
        TilesStorage    = new DataStorage<TileData>   ();
        MapsStorage     = new DataStorage<MapData>    ();
        UnitsStorage    = new DataStorage<UnitData>   ();
        
        LayersStorage  .AfterAdded += (d) => PushProjectEdit(EditEntry.Layers, LayersStorage, d);
        TexturesStorage.AfterAdded += (d) => PushProjectEdit(EditEntry.Textures, TexturesStorage, d);
        TilesStorage   .AfterAdded += (d) => PushProjectEdit(EditEntry.Tiles,  TilesStorage, d);
        MapsStorage    .AfterAdded += (d) => PushProjectEdit(EditEntry.Maps, MapsStorage, d);
        UnitsStorage   .AfterAdded += (d) => PushProjectEdit(EditEntry.Units, UnitsStorage, d);
        
        LayersStorage  .AfterRemoved += (d) => PushProjectEdit(EditEntry.Textures, TexturesStorage, d);
        TexturesStorage.AfterRemoved += (d) => PushProjectEdit(EditEntry.Textures, TexturesStorage, d);
        TilesStorage   .AfterRemoved += (d) => PushProjectEdit(EditEntry.Tiles,  TilesStorage, d);
        MapsStorage    .AfterRemoved += (d) => PushProjectEdit(EditEntry.Maps, MapsStorage, d);
        UnitsStorage   .AfterRemoved += (d) => PushProjectEdit(EditEntry.Units, UnitsStorage, d);
        
        storages.Add(typeof(LayerData),   LayersStorage);
        storages.Add(typeof(TextureData), TexturesStorage);
        storages.Add(typeof(TileData),    TilesStorage);
        storages.Add(typeof(MapData),     MapsStorage);
        storages.Add(typeof(UnitData),    UnitsStorage);
    }
    
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
    
    public TextureData? GetTexture(string id) => TexturesStorage.Get(id);
    public TileData?    GetTile(string id)    => TilesStorage   .Get(id);
    public MapData?     GetMap(string id)     => MapsStorage    .Get(id);
    public UnitData?    GetUnit(string id)    => UnitsStorage   .Get(id);

    public void AddTexture(TextureData textureData) => TexturesStorage.Add(textureData);
    public void AddTile(TileData tileData)          => TilesStorage   .Add(tileData);
    public void AddMap(MapData mapData)             => MapsStorage    .Add(mapData);
    public void AddUnit(UnitData unitData)          => UnitsStorage   .Add(unitData);

    public void AddTextures(TextureData[] data) => TexturesStorage.AddMultiple(data);
    public void AddTiles(TileData[] data)       => TilesStorage   .AddMultiple(data);
    public void AddUnits(UnitData[] data)       => UnitsStorage   .AddMultiple(data);
    public void AddMaps(MapData[] data)         => MapsStorage    .AddMultiple(data);

    public void RemoveTexture(TextureData textureData) => TexturesStorage.Remove(textureData);
    public void RemoveTile(TileData tileData)          => TilesStorage   .Remove(tileData);
    public void RemoveMap(MapData mapData)             => MapsStorage    .Remove(mapData);
    public void RemoveUnit(UnitData unitData)          => UnitsStorage   .Remove(unitData);

    public void RemoveAllTextures() => TexturesStorage.RemoveAll();
    public void RemoveAllTiles()    => TilesStorage   .RemoveAll();
    public void RemoveAllMaps()     => MapsStorage    .RemoveAll();
    public void RemoveAllUnits()    => UnitsStorage   .RemoveAll();
    
    public struct EditEntry
    {
        //generic ids
        public const string Layers   = "layers_edit";
        public const string Textures = "texture_edit";
        public const string Tiles    = "tiles_edit";
        public const string Maps     = "maps_edit";
        public const string Units    = "units_edit";
        
        public readonly string Tag;
        public readonly object Where;
        public readonly object Who;

        public EditEntry(string tag, object where, object who)
        {
            this.Tag = tag;
            this.Where = where;
            this.Who = who;
        }

    }
    
    public void PushProjectEdit(string tag, object where, object who)
    {
        OnEdited?.Invoke(new EditEntry(tag, where, who));
    }

    public sealed class DataStorage<TData>
        where TData : LEditorDataUnit
    {
        public TData[] Data => dataMap.Values.ToArray();
        private readonly Dictionary<string, TData> dataMap = new();

        public readonly Predicate<TData> CanBeAddedChecker;
        public event Action<TData> AfterAdded;
        
        public readonly Predicate<TData> CanBeRemovedChecker;
        public event Action<TData> BeforeRemoved;
        public event Action<TData> AfterRemoved;
        
        public DataStorage() { }

        public DataStorage(Predicate<TData> canBeAddedChecker, Action<TData> afterAdded, Predicate<TData> canBeRemovedChecker, Action<TData> beforeRemoved, Action<TData> afterRemoved)
        {
            CanBeAddedChecker    = canBeAddedChecker;
            CanBeRemovedChecker  = canBeRemovedChecker;
            
            AfterAdded    += afterAdded;
            BeforeRemoved += beforeRemoved;
            AfterRemoved  += afterRemoved;
        }

        public DataStorage(Action<TData> afterAdded, Action<TData> afterRemoved)
        {
            AfterAdded   += afterAdded;
            AfterRemoved += afterRemoved;
        }

        public DataStorage(Predicate<TData> canBeAddedChecker, Predicate<TData> canBeRemovedChecker)
        {
            CanBeAddedChecker   = canBeAddedChecker;
            CanBeRemovedChecker = canBeRemovedChecker;
        }
        
        public TData? Get(string id)
        {
            if (!dataMap.ContainsKey(id))
            {
                return null;
            }
            
            return dataMap[id];
        }

        public void Add(TData data)
        {
            if(!CanBeAdded(data))
            {
                return;
            }
            
            dataMap.Add(data.ID, data);
            Console.WriteLine("with " + data.ID + $" {dataMap.Count} {Data.Length}");
            AfterAdded?.Invoke(data);
        }

        public void Remove(TData data)
        {
            if (!CanBeRemoved(data))
            {
                return;
            }

            BeforeRemoved?.Invoke(data);
            dataMap.Remove(data.ID);
            AfterRemoved?.Invoke(data);
        }

        public void RemoveAll()
        {
            foreach (TData data in dataMap.Values)
            {
                Remove(data);
            }
        }

        public void AddMultiple(TData[] data)
        {
            foreach (TData d in data)
            {
                Add(d);
            }
        }

        private bool CanBeAdded(TData data)
        {
            return !dataMap.ContainsKey(data.ID) && (CanBeAddedChecker == null || CanBeAddedChecker.Invoke(data));
        }

        private bool CanBeRemoved(TData data)
        {
            return dataMap.ContainsKey(data.ID) && (CanBeRemovedChecker == null || CanBeRemovedChecker.Invoke(data));
        }
    }
}