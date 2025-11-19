namespace leditor.root;

public sealed class ProjectEnvironment
{
    public ProjectData Project;
    //modules
    public readonly Toolset       Toolset = new();
    public readonly UnitSwitch    UnitSwitch = new();
    public string OriginalPath { get; private set; }

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

    public void SaveProjectAtOriginalPath()
    {
        File.WriteAllText(OriginalPath, Project.Export());
    }


    public void InitializeEnvironment()
    {
        Toolset.BuildGUI();
        UnitSwitch.BuildGUI();
    }

    public void ClearEnvironment()
    {
        
    }
}