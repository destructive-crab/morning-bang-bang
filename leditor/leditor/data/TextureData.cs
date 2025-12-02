using Newtonsoft.Json;
using SFML.Graphics;

namespace leditor.root;

[JsonObject(MemberSerialization.OptIn)]
public sealed class TextureData : LEditorDataUnit
{
    public const string NO_TEXTURE = "NO_TEXTURE_DEFINED";

    [PathField] [JsonProperty] public string PathToTexture = "";
    
    [JsonProperty] public int StartX;
    [JsonProperty] public int StartY;
    
    [JsonProperty] public int Width;
    [JsonProperty] public int Height;
    
    [JsonProperty] public int PivotX; //from 0 to 100
    [JsonProperty] public int PivotY; //from 0 to 100
    
    public Rect TextureRect => new Rect(StartX, StartY, Width, Height);

    //json requirement 
    public TextureData()
    {
        ID = NO_TEXTURE;
        SetTexture(NO_TEXTURE);
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

    public override bool ValidateExternalDataChange()
    {
        SetTexture(PathToTexture);

        PivotX = Math.Clamp(PivotX, 0, 100);
        PivotY = Math.Clamp(PivotY, 0, 100);
        
        return true;
    }

    public override void CopyDataFrom(LEditorDataUnit from)
    {
        ID = from.ID;
        
        if (from is not TextureData textureData) return;
        
        StartX = textureData.StartX;
        StartY = textureData.StartY;
        Width = textureData.Width;
        Height = textureData.Height;
        
        SetTexture(textureData.PathToTexture);
    }

    public void SetTexture(string path)
    {
        if(!File.Exists(path))
        {
            PathToTexture = NO_TEXTURE;
        }
        else
        {
            if (path == PathToTexture) return;
             
            PathToTexture = path;           
        }

        Texture texture = RenderCacher.GetTexture(PathToTexture);

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