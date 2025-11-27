using Newtonsoft.Json;
using SFML.Graphics;

namespace leditor.root;

[JsonObject(MemberSerialization.OptIn)]
public sealed class TextureData
{
    [JsonProperty] public string textureID;
    [JsonProperty] public string pathToTexture { get; private set; }
    
    [JsonProperty] public int StartX;
    [JsonProperty] public int StartY;
    
    [JsonProperty] public int Width;
    [JsonProperty] public int Height;
    
    public Rect rectangle => new Rect(StartX, StartY, Width, Height);
    
    //json requirement 
    public TextureData() { }
    
    public TextureData(string id, string path)
    {
        textureID = id;
        SetTexture(path);
    }
    
    public TextureData(string id, string path, Rect rectangle)
    {
        textureID = id;
        SetTexture(path);

        StartX = rectangle.StartX;
        StartY = rectangle.StartY;
        Width = rectangle.Width;
        Height = rectangle.Height;
    }

    public void SetTexture(string path)
    {
        pathToTexture = path;
        Texture texture = RenderCacher.GetTexture(path);
        
        Width = (int)texture.Size.X;
        Height = (int)texture.Size.Y;
    }
}