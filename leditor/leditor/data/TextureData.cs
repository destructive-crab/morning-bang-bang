namespace leditor.root;

public sealed class TextureData
{
    public readonly string textureID;
    public readonly string pathToTexture;
    
    public readonly Rect rectangle;
    public readonly bool isCropped;

    public TextureData(string id, string path)
    {
        textureID = id;
        pathToTexture = path;

        isCropped = false;
    }
    
    public TextureData(string id, string path, Rect rectangle)
    {
        textureID = id;
        pathToTexture = path;

        isCropped = true;
        this.rectangle = rectangle;
    }
}