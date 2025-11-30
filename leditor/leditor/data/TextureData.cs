using Newtonsoft.Json;
using SFML.Graphics;

namespace leditor.root;

[JsonObject(MemberSerialization.OptIn)]
public sealed class TextureData : LEditorDataUnit
{
    public const string NO_TEXTURE = "NO_TEXTURE_DEFINED";
    public static string GetNoTextureFound() 
        => "C:\\Users\\destructive_crab\\dev\\band-bang\\leditor\\leditor\\assets\\NO_TEXTURE.png";
    
    [PathField] [JsonProperty] public string PathToTexture = "";
    
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
        ID = NO_TEXTURE;
        SetTexture(GetNoTextureFound());
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
        if(!File.Exists(path))
        {
            Console.WriteLine("AAA");
            PathToTexture = GetNoTextureFound();
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