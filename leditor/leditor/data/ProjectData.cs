using System.Numerics;
using Raylib_cs;

namespace leditor.root;

public class ProjectData
{
    public TileData[] Tiles => tiles.ToArray();
    public UnitData[] Units => units.ToArray();
    
    public readonly int TILE_HEIGHT = 512;
    public readonly int TILE_WIDTH = 512;
    
    private readonly List<TextureData> textures = new();
    private readonly List<TileData> tiles = new();
    private readonly List<TilemapData> tilemaps = new();
    public readonly List<UnitData> units = new();

    private readonly Dictionary<string, TextureData> texturesMap = new();
    private readonly Dictionary<string, TileData> tilesMap = new();
    private readonly Dictionary<string, TilemapData> tilemapsMap = new();
    private readonly Dictionary<string, UnitData> unitsMap = new();
    
    public void AddTile(string tileID, TextureData data)
    {
        TileData newTile = new TileData(tileID);
        newTile.texture_id = data.textureID;
        tiles.Add(newTile);
        tilesMap.Add(tileID, newTile);
    }

    public void AddMap(string mapID, KeyValuePair<Vector2, string>[] tiles)
    {
        TilemapData tilemapData = new TilemapData(tiles);
        tilemaps.Add(tilemapData);
        tilemapsMap.Add(mapID, tilemapData);
    }

    public void AddUnit(string unitID, string mapID, string overrideID)
    {
        UnitData unit = new UnitData(unitID, mapID, overrideID);
        
        units.Add(unit);
        unitsMap.Add(unitID, unit);
    }

    public TextureData[] CreateTilesFromTileset(string generalID, string tilesetPath)
    {
        List<TextureData> result = new();
        Image img = AssetsStorage.GetImage(tilesetPath);

        int setW = img.Width / TILE_WIDTH;
        int setH = img.Height / TILE_HEIGHT;

        int idIndex = 1;
        for (int w = 0; w < setW; w++)
        {
            for (int h = 0; h < setH; h++)
            {
                Rectangle rec = new Rectangle(new Vector2(TILE_WIDTH * w, TILE_HEIGHT * h), TILE_WIDTH, TILE_HEIGHT);
                
                Image croppedGuiImage = Raylib.ImageCopy(img);
                Raylib.ImageCrop(ref croppedGuiImage, rec);

                TextureData textureData = AddTexture(generalID + $"_{idIndex}", tilesetPath, rec);

                Texture2D croppedTex = Raylib.LoadTextureFromImage(croppedGuiImage);
                AssetsStorage.RegisterCrop(textureData.textureID, croppedTex);
                
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
        textures.Add(newTexture);
        texturesMap.Add(id, newTexture);
        
        return newTexture;
    }
    
    public TextureData AddTexture(string id, string texturePath, Rectangle rectangle)
    {
        TextureData newTexture = new TextureData(id, texturePath, rectangle);
        textures.Add(newTexture);
        texturesMap.Add(id, newTexture);
        
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
}