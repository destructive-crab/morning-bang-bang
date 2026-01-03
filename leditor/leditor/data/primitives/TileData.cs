using Newtonsoft.Json;

namespace leditor.root;

[JsonObject(MemberSerialization.OptIn)]
public sealed class TileData : LEditorDataUnit
{
    public const string EMPTY = "empty";
    
    [JsonProperty] public TextureReference TextureReference;
    [JsonProperty] public LayerID[] AllowLayers = LayerID.AllLayers;

    public TileData()
    {
        ID = EMPTY;
        TextureReference = new("");
    }

    public TileData(string id)
    {
        ID = id;
    }

    public override bool ValidateExternalDataChange()
    {
        if(!UTLS.ValidString(ID)) return false;
        
        List<LayerID> validatedLayers = new(AllowLayers);
        
        foreach (LayerID layer in AllowLayers)
        {
            if (!LayerID.AllLayers.Contains(layer))
            {
                validatedLayers.Remove(layer);
            }
        }

        AllowLayers = validatedLayers.ToArray();

        return true;
    }

    public override void CopyDataFrom(LEditorDataUnit from)
    {
        ID = from.ID;
        
        if (from is not TileData tileData) return;
        
        TextureReference = tileData.TextureReference;
    }
}