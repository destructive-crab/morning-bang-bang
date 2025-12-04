using SFML.Graphics;

namespace leditor.root;

public static class AssetsStorage
{
    public static Image GetInvalidImage()
    {
        if (invalidImage != null) return invalidImage;
        
        Color[,] pixels = new Color[100, 100];
        
        for (int x = 0; x < 100; x++)
        {
            for (int y = 0; y < 100; y++)
            {
                pixels[x, y] = Color.Magenta;
            }
        }
        invalidImage = new Image(pixels);
        
        return invalidImage;
    }
    public static Texture GetInvalidTexture()
    {
        if (invalidTexture == null)
        {
            invalidTexture = new Texture(GetInvalidImage());
        }

        return invalidTexture;
    }

    private static readonly Dictionary<string, SFML.Graphics.Image> cache_img   = new();

    private static Texture invalidTexture = null;
    private static Image invalidImage = null;
    
    public static void RegisterImage(string id, Image img)
    {
        cache_img.Add(id, img);
    }
    
    /// <summary>
    /// Returns cached instance of image. No image editing supposed to be done on that instance
    /// </summary>
    public static Image GetImageAtPath(string path)
    {
        if (!File.Exists(path))
        {
            return GetInvalidImage();
        }
        
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