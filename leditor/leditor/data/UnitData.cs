namespace leditor.root;

public class UnitData
{
    public const string NO_OVERRIDE = "NO_OV";
    public string UnitID;
    public string MapID;
    public string OverrideID;

    public UnitData(string unitId, string mapId, string overrideId)
    {
        UnitID = unitId;
        MapID = mapId;
        OverrideID = overrideId;
    }
}