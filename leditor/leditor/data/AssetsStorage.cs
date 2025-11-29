using SFML.Graphics;

namespace leditor.root;

public static class AssetsStorage
{
    private static readonly Dictionary<string, SFML.Graphics.Image> cache_img   = new();


    public static void RegisterImage(string id, Image img)
    {
        cache_img.Add(id, img);
    }
    
    public static Image GetImageAtPath(string path)
    {
        if (cache_img.ContainsKey(path))
        {
            return cache_img[path];
        }

        Image img = new Image(path);
        
        RegisterImage(path, img);
        return img;
    }

    public static Image GetImage(TextureData data)
    {
        return GetImageAtPath(data.PathToTexture);
    }
}