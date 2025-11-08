using System.Collections.Generic;
using Raylib_cs;

namespace leditor.root;

public static class AssetsStorage
{
    private static readonly Dictionary<string, Raylib_cs.Texture2D> cache_tex   = new();
    private static readonly Dictionary<string, Raylib_cs.Image>     cache_img   = new();
    private static readonly Dictionary<string, Raylib_cs.Texture2D> cache_crops = new();

    public static void RegisterCrop(string id, Texture2D tex)
    {
        cache_crops.Add(id, tex);
    }

    public static Texture2D GetCrop(string id)
    {
        return cache_crops[id];
    }
    
    public static Image GetImage(string path)
    {
        if (cache_tex.ContainsKey(path))
        {
            return cache_img[path];
        }

        Image img = Raylib.LoadImage(path); 
        cache_img.Add(path, img);
        return img;
    }
    
    public static Texture2D GetRawTexture(string path)
    {
        if (cache_tex.ContainsKey(path))
        {
            return cache_tex[path];
        }

        Texture2D tex = Raylib.LoadTexture(path); 
        cache_tex.Add(path, tex);
        return tex;
    }

    public static Texture2D GetTexture(TextureData data)
    {
        if (data.isCropped)
        {
            return cache_crops[data.textureID];
        }
        
        return GetRawTexture(data.pathToTexture);
    }
}