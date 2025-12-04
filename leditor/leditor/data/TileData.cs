namespace leditor.root;

public sealed class TileData : LEditorDataUnit
{
    public const string EMPTY = "empty";
    
    [TextureDataReference] public string TextureID;

    public TileData()
    {
        ID = EMPTY;
        TextureID = EMPTY;
    }

    public TileData(string id)
    {
        ID = id;
    }

    public override bool ValidateExternalDataChange() => true;

    public override void CopyDataFrom(LEditorDataUnit from)
    {
        ID = from.ID;
        
        if (from is not TileData tileData) return;
        
        TextureID = tileData.TextureID;
    }
}