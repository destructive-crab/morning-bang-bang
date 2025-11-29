using Newtonsoft.Json;

namespace leditor.root;

[JsonObject(MemberSerialization.OptIn)]
public class UnitData : LEditorDataUnit
{
    public const string NO_OVERRIDE = "NO_OV";
    [JsonProperty] public string MapID = "_";
    [JsonProperty] public string OverrideID = NO_OVERRIDE;

    public UnitData() { }

    public UnitData(string id, string mapId, string overrideId)
    {
        ID = id;
        MapID = mapId;
        OverrideID = overrideId;
    }

    public override bool ValidateExternalDataChange()
    {
        return true;
    }

    public override void CopyDataFrom(LEditorDataUnit from)
    {
    }
}