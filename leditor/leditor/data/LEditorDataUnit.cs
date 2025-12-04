using Newtonsoft.Json;

namespace leditor.root;

public abstract class LEditorDataUnit
{
    [JsonProperty] public string ID = "_";
    public abstract bool ValidateExternalDataChange();
    public abstract void CopyDataFrom(LEditorDataUnit from);
}