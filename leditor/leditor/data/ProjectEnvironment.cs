namespace leditor.root;

public sealed class ProjectEnvironment
{
    public ProjectData Project;
    //modules
    public readonly Toolset       Toolset = new();
    public readonly UnitSwitcher    UnitSwitcher = new();
    public string OriginalPath { get; private set; }

    public void OpenEmptyProject()
    {
        Project = new ProjectData();
        
        TextureData tex = Project.AddTexture("red", "C:\\Users\\destructive_crab\\dev\\band-bang\\leditor\\leditor\\assets\\tests\\red.png");
        Project.AddTile("red", tex);
    
        Project.CreateTilesFromTileset("wall_up",
            "C:\\Users\\destructive_crab\\dev\\band-bang\\leditor\\leditor\\assets\\tests\\wall_up.png");
                    
        
        OriginalPath = string.Empty;
        InitializeEnvironment();
    }
    public void OpenProjectAtPath(string path)
    {
        if (File.Exists(path))
        {
            string content = File.ReadAllText(path);
            Project = ProjectData.Import(content);
            //checks if valid
            OriginalPath = path;
        }
    }

    public void SaveProjectAtPath(string path)
    {
        if (File.Exists(path))
        {
        }
        else
        {
            File.Create(path);
            File.WriteAllText(path, Project.Export());
        }
        
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

    public void InitializeEnvironment()
    {
    }

    public void ClearEnvironment()
    {
        
    }
}