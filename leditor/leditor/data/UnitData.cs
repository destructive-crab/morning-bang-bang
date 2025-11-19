using Newtonsoft.Json;

namespace leditor.root;

[JsonObject(MemberSerialization.OptIn)]
public class UnitData
{
    public const string NO_OVERRIDE = "NO_OV";
    [JsonProperty] public string UnitID;
    [JsonProperty] public string MapID;
    [JsonProperty] public string OverrideID;

    public UnitData()
    {
    }

    public UnitData(string unitId, string mapId, string overrideId)
    {
        UnitID = unitId;
        MapID = mapId;
        OverrideID = overrideId;
    }
}