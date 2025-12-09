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
        if(!UTLS.ValidString(ID)) return false;
        return true;
    }

    public override void CopyDataFrom(LEditorDataUnit from)
    {
        ID = from.ID;
        
        if (from is not UnitData unitData) return;
        
        MapID = unitData.MapID;
        OverrideID = unitData.OverrideID;
    }
}