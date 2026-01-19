namespace leditor.root;

public struct IntVec
{
    public int X;
    public int Y;

    public IntVec(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public struct LayerID
{
    public static readonly string[]  AllIDs = { FloorID, FloorOverlayID, ObstaclesID, ObstaclesOverlayID };
    public const string FloorID             = "floor";
    public const string FloorOverlayID      = "floor_overlay";
    public const string ObstaclesID         = "obstacles";
    public const string ObstaclesOverlayID  = "obstacles_overlay";
    
    public static readonly LayerID[] AllLayers = { new LayerID(FloorID), new LayerID(FloorOverlayID), new LayerID(ObstaclesID), new LayerID(ObstaclesOverlayID)};
    public static readonly LayerID Floor            = new(FloorID);
    public static readonly LayerID FloorOverlay     = new(FloorOverlayID);
    public static readonly LayerID Obstacles        = new(ObstaclesID);
    public static readonly LayerID ObstaclesOverlay = new(ObstaclesOverlayID);

    public string ID;

    public LayerID(string id)
    {
        ID = id;
    }

    public bool Equals(LayerID other)
    {
        return ID == other.ID;
    }

    public override bool Equals(object? obj)
    {
        return obj is LayerID other && Equals(other);
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }
}

public struct FilePath
{
    public static readonly FilePath NONE = new FilePath("NONE");
    public string Path;

    public FilePath(string path)
    {
        Path = path;
    }
}

public struct FolderPath
{
    public string Path;

    public FolderPath(string path)
    {
        Path = path;
    }
}

public struct TextureReference 
{
    public const string INVALID_ID = "INVALID";
    public static readonly TextureReference INVALID = new TextureReference(INVALID_ID);
    
    public string ID;

    public TextureReference(string id)
    {
        ID = id;
    }

    public bool Equals(TextureReference other)
    {
        return ID == other.ID;
    }

    public override bool Equals(object? obj)
    {
        return obj is TextureReference other && Equals(other);
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }
}

public struct TileReference
{
    public const string INVALID_ID = "INVALID";
    public static readonly TextureReference INVALID = new TextureReference(INVALID_ID);
   
    public string ID;

    public TileReference(string id)
    {
        ID = id;
    }
}