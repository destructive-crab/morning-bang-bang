using leditor.UI;

namespace leditor.root;

public abstract class EditorDisplay
{
    public abstract UIHost Host { get; }
    public virtual void Tick() {}
}