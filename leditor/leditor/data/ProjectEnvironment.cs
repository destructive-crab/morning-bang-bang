namespace leditor.root;

public sealed class ProjectEnvironment
{
    public bool IsProjectAvailable => Project != null;
    
    public ProjectData Project;
    
    public const string INVALID_TEXTURE_ID = "invalid_texture";
    public const string INVALID_TILE_ID = "invalid_tile";
    
    //modules
    public Toolset         Toolset;
    public readonly UnitSwitcher    UnitSwitcher = new();
    public string OriginalPath { get; private set; }

    public void OpenEmptyProject()
    {
        Project = new ProjectData();
        
        OriginalPath = string.Empty;
        InitializeEnvironment();
    }
    public bool OpenProjectAtPath(string path)
    {
        if (File.Exists(path))
        {
            string content = File.ReadAllText(path);
            Project = ProjectData.Import(content);
            //checks if valid
            OriginalPath = path;
            InitializeEnvironment();
            return true;
        }

        ClearEnvironment();
        return false;
    }

    public void SaveProjectAtPath(string path)
    {
        if (!path.EndsWith(".lep")) path += ".lep";
        
        if (!File.Exists(path))
        {
            File.Create(path).Close();
        }
        
        File.WriteAllText(path, Project.Export());
        
        OriginalPath = path;
    }
    public void SaveProject()
    {
        if (OriginalPath == string.Empty)
        {
            OriginalPath = UTLS.OpenSaveProjectDialog();
        }
        
        File.WriteAllText(OriginalPath, Project.Export());
    }

    public TextureData GetInvalidTexture()
    {
        TextureData invalidTexture = new TextureData();
        return invalidTexture;
    }
    public TileData GetInvalidTile()
    {
        TileData invalidTile = new();
        
        invalidTile.ID = INVALID_TILE_ID;
        invalidTile.TextureID = INVALID_TEXTURE_ID;

        return invalidTile;
    }
    
    public TextureData GetTexture(string textureID)
    {
        if (!IsProjectAvailable) return GetInvalidTexture();

        TextureData? textureData = Project.GetTexture(textureID);

        if (textureData == null) return GetInvalidTexture();

        return textureData;
    }
    
    public TileData GetTile(string tileID)
    {
        if (!IsProjectAvailable) return GetInvalidTile();
        
        TileData? tileFromProject = Project.GetTile(tileID);
        
        if (tileFromProject == null) return GetInvalidTile();

        return tileFromProject;
    }

    public MapData? GetMap(string mapID) => Project.GetMap(mapID);
    public UnitData? GetUnit(string unitID) => Project.GetUnit(unitID);

    public void InitializeEnvironment()
    {
        Toolset = new(Project);
    }

    public void ClearEnvironment()
    {
        
    }
}