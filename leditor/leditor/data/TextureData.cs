using Newtonsoft.Json;
using SFML.Graphics;

namespace leditor.root;

[JsonObject(MemberSerialization.OptIn)]
public sealed class TextureData : LEditorDataUnit
{
    [PathField] [JsonProperty] public string PathToTexture = "";
    
    [JsonProperty] public int StartX;
    [JsonProperty] public int StartY;
    
    [JsonProperty] public int Width;
    [JsonProperty] public int Height;
    private string NOT_DEFINED = "NOT_DEFINED";

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
        ID = NOT_DEFINED;
        SetTexture(NOT_DEFINED);
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
        if (path == PathToTexture) return;
        
        PathToTexture = path;
        if(path == NOT_DEFINED) return;
        
        Texture texture = RenderCacher.GetTexture(path);

        if (Width == 0 || Width > (int)texture.Size.X)
        {
            Width = (int)texture.Size.X;
        }
        if (Height == 0 || Height > (int)texture.Size.Y)
        {
            Height = (int)texture.Size.Y;
        }
    }
}