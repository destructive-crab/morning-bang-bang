using Newtonsoft.Json;

namespace leditor.root;

[JsonObject(MemberSerialization.OptIn)]
public sealed class TextureData
{
    [JsonProperty] public string textureID;
    [JsonProperty] public string pathToTexture;
    
    [JsonProperty] public int StartX;
    [JsonProperty] public int StartY;
    
    [JsonProperty] public int Width;
    [JsonProperty] public int Height;
    
    public Rect rectangle => new Rect(StartX, StartY, Width, Height);

    public TextureData()
    {
        
    }
    
    public TextureData(string id, string path)
    {
        textureID = id;
        pathToTexture = path;
    }
    
    public TextureData(string id, string path, Rect rectangle)
    {
        textureID = id;
        pathToTexture = path;

        StartX = rectangle.StartX;
        StartY = rectangle.StartY;
        Width = rectangle.Width;
        Height = rectangle.Height;
    }
}