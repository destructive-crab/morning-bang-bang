using leditor.root;

namespace leditor.editor;

public class ProjectDataEditsRegistry
{
    public event Action<ProjectData.EditEntry> OnProjectContentEdit;
    public event Action<string, object> OnObjectMarkedAsDirty;
    public event Action<object> OnObjectUnmarkedAsDirty;
    
    private ProjectEnvironment environment;
    private ProjectData boundWith;

    private readonly List<ProjectData.EditEntry> edits = new();
    private readonly List<ProjectData.EditEntry> editsOnFrame = new();

    private readonly Dictionary<object, string> markedAsEdited = new();
    private readonly List<object> dirty = new();
    
    public ProjectDataEditsRegistry(ProjectEnvironment environment, ProjectData boundWith)
    {
        this.environment = environment;
        this.boundWith = boundWith;

        BindCallbacks();
    }

    private void BindCallbacks()
    {
        boundWith.OnEdited += ProcessEdit;
    }

    private void ProcessEdit(ProjectData.EditEntry edit)
    {
        editsOnFrame.Add(edit);
    }

    /// <summary>
    /// Usually we want to provide object id as tag
    /// </summary>
    public void MarkAsDirty(string tag, object o)
    {
        if (dirty.Contains(o)) return;
        
        markedAsEdited.Add(o, tag);
        dirty.Add(o);
        OnObjectMarkedAsDirty?.Invoke(tag, o);
    }

    public void UnmarkAsDirty(object o)
    {
        markedAsEdited.Remove(o);
        dirty.Remove(o);
        OnObjectUnmarkedAsDirty?.Invoke(o);
    }

    public bool IsDirty(object o) => dirty.Contains(o);

    public ProjectData.EditEntry[] PullThisFrameEdits()
    {
        ProjectData.EditEntry[] edits = new ProjectData.EditEntry[editsOnFrame.Count];
        for (var i = 0; i < edits.Length; i++)
        {
            ProjectData.EditEntry edit = editsOnFrame[i];
            edits[i] = edit;
            OnProjectContentEdit?.Invoke(edit);
        }

        editsOnFrame.Clear();
        this.edits.AddRange(edits);
        
        return edits;
    }

    public void ClearAllRegistry()
    {
        foreach (object o in dirty.ToArray())
        {
            UnmarkAsDirty(o);
        }
        edits.Clear();
        editsOnFrame.Clear();
    }
}