using SFML.Graphics;

namespace leditor.root;

public static class RenderCacher
{
    private static readonly Dictionary<string, Texture> texturesMap = new();
    private static readonly Dictionary<string, Sprite> spritesMap = new();
    
    public static Texture GetTexture(string path)
    {
        if (path.StartsWith("assets"))
        {
            path = Path.Combine(App.GeneralPath, path);
        }
        
        if (texturesMap.TryGetValue(path, out Texture tex))
        {
            return tex;
        }
        
        Image rawImage = AssetsStorage.GetImageAtPath(path);
        Texture texture = new Texture(rawImage);
        
        texturesMap.Add(path, texture);

        return texture;
    }

    public static void CacheSpriteWithID(string id, Sprite sprite)
    {
        if (spritesMap.ContainsKey(id))
        {
            spritesMap[id] = sprite;
        }
        else
        {
            spritesMap.Add(id, sprite);
        }
    }

    public static void TryGetSprite()
    {
        
    }
}