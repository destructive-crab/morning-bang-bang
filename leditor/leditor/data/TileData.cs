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
        this.ID = id;
    }

    public override bool ValidateExternalDataChange()
    {
        return true;
    }

    public override void CopyDataFrom(LEditorDataUnit from)
    {
        if (from is TileData tileData)
        {
            ID = tileData.ID;
            TextureID = tileData.TextureID;
        }
    }
}