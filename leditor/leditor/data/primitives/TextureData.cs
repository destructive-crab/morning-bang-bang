using Newtonsoft.Json;
using SFML.Graphics;

namespace leditor.root;

[JsonObject(MemberSerialization.OptIn)]
public sealed class TextureData : LEditorDataUnit
{
    public const string NO_TEXTURE = "NO_TEXTURE_DEFINED";

    [JsonProperty] public FilePath PathToTexture;

    [JsonProperty] public IntVec Start;
    [JsonProperty] public IntVec Size;
    [JsonProperty] public IntVec Pivot;
    
    public Rect TextureRect => new Rect(Start.X, Start.Y, Size.X, Size.Y);

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

        Start.X = rectangle.StartX;
        Start.Y = rectangle.StartY;
        Size.X = rectangle.Width;
        Size.Y = rectangle.Height;
    }

    public override bool ValidateExternalDataChange()
    {
        if(!UTLS.ValidString(ID)) return false;
        SetTexture(PathToTexture.Path);

        Pivot.X = Math.Clamp(Pivot.X, 0, 100);
        Pivot.Y = Math.Clamp(Pivot.Y, 0, 100);
        
        return true;
    }

    public override void CopyDataFrom(LEditorDataUnit from)
    {
        ID = from.ID;
        
        if (from is not TextureData textureData) return;
        
        Start.X = textureData.Start.X;
        Start.Y = textureData.Start.Y;
        Size.X = textureData.Size.X;
        Size.Y = textureData.Size.Y;
        Pivot.X = textureData.Pivot.X;
        Pivot.Y = textureData.Pivot.Y;
        
        SetTexture(textureData.PathToTexture.Path);
    }

    public void SetTexture(string path)
    {
        if(!File.Exists(path))
        {
            PathToTexture = FilePath.NONE;
        }
        else
        {
            if (path == PathToTexture.Path) return;
             
            PathToTexture = new FilePath(path);           
        }

        Texture texture = RenderCacher.GetTexture(PathToTexture.Path);

        if (Size.X == 0 || Size.X > (int)texture.Size.X)
        {
            Size.X = (int)texture.Size.X;
        }
        if (Size.Y == 0 || Size.Y > (int)texture.Size.Y)
        {
            Size.Y = (int)texture.Size.Y;
        }
    }
}