using Raylib_cs;

namespace leditor.root;

public sealed class TextureData
{
    public readonly string textureID;
    public readonly string pathToTexture;
    
    public readonly Rectangle rectangle;
    public readonly bool isCropped;

    public TextureData(string id, string path)
    {
        textureID = id;
        pathToTexture = path;

        isCropped = false;
    }
    
    public TextureData(string id, string path, Rectangle rectangle)
    {
        textureID = id;
        pathToTexture = path;

        isCropped = true;
        this.rectangle = rectangle;
    }
}