using Newtonsoft.Json;

namespace leditor.root;


[JsonObject(MemberSerialization.OptIn)]
public sealed class TileData : LEditorDataUnit
{

    public const string EMPTY = "empty";
    
    [TextureDataReference] 
    [JsonProperty] public string TextureID;
    
    [LayersList]
    [JsonProperty] public string[] AllowLayers = MapData.AllLayers;

    public TileData()
    {
        ID = EMPTY;
        TextureID = EMPTY;
    }

    public TileData(string id)
    {
        ID = id;
    }

    public override bool ValidateExternalDataChange()
    {
        List<string> validatedLayers = new(AllowLayers);
        
        foreach (string layer in AllowLayers)
        {
            if (!MapData.AllLayers.Contains(layer))
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
        
        TextureID = tileData.TextureID;
    }
}