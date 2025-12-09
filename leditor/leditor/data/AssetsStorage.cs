using SFML.Graphics;

namespace leditor.root;

public static class AssetsStorage
{
    public static Image GetInvalidImage()
    {
        if (invalidImage != null) return invalidImage;

        int width = 100;
        int height = 100;
        
        if (App.LeditorInstance.ProjectEnvironment.IsProjectAvailable)
        {
            width = App.LeditorInstance.Project.TILE_WIDTH;
            height = App.LeditorInstance.Project.TILE_HEIGHT;
        }
        Color[,] pixels = new Color[width, height];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
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