namespace leditor.root;

public sealed class TileData
{
    public const string EMPTY = "empty";
    
    public string id;
    public string texture_id;

    public TileData()
    {
    }

    public TileData(string id)
    {
        this.id = id;
    }
}