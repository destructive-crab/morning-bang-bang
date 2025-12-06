using Newtonsoft.Json;

namespace leditor.root.export;

[JsonObject(MemberSerialization.OptIn)]
public class ProjectDataExportRepresentation
{
    [JsonProperty] public MapData[] Tilemaps;
    [JsonProperty] public TileData[] Tiles;
    [JsonProperty] public UnitData[] Units;
    [JsonProperty] public TextureData[] Textures;

    public ProjectDataExportRepresentation()
    {
    }

    public ProjectDataExportRepresentation(MapData[] tilemaps, TileData[] tiles, UnitData[] units, TextureData[] textures)
    {
        Tilemaps = tilemaps;
        Tiles = tiles;
        Units = units;
        Textures = textures;
    }
}