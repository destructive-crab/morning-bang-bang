using Newtonsoft.Json;
using SFML.Graphics;

namespace leditor.root;

[JsonObject(MemberSerialization.OptIn)]
public sealed class TextureData : LEditorDataUnit
{
    [JsonProperty] public string PathToTexture = "";
    
    [JsonProperty] public int StartX;
    [JsonProperty] public int StartY;
    
    [JsonProperty] public int Width;
    [JsonProperty] public int Height;
    
    public Rect rectangle => new Rect(StartX, StartY, Width, Height);

    public override bool ValidateExternalDataChange()
    {
        SetTexture(PathToTexture);
        return true;
    }

    public override void CopyDataFrom(LEditorDataUnit from)
    {
        if (from is not TextureData casted) return;
        
        ID = casted.ID;
        SetTexture(casted.PathToTexture);
        StartX = casted.StartX;
        StartY = casted.StartY;
        Width = casted.Width;
        Height = casted.Height;
    }

    //json requirement 
    public TextureData()
    {
        ID = "NOT DEFINED";
        SetTexture("NOT DEFINED");
    }
    
    public TextureData(string id, string path)
    {
        ID = id;
        SetTexture(path);
    }
    
    public TextureData(string id, string path, Rect rectangle)
    {
        ID = id;
        SetTexture(path);

        StartX = rectangle.StartX;
        StartY = rectangle.StartY;
        Width = rectangle.Width;
        Height = rectangle.Height;
    }

    public void SetTexture(string path)
    {
        PathToTexture = path;
        if(path == "NOT DEFINED") return;
        
        Texture texture = RenderCacher.GetTexture(path);
        
        Width = (int)texture.Size.X;
        Height = (int)texture.Size.Y;
    }
}