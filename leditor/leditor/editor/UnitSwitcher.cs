namespace leditor.root;

public class UnitSwitcher
{
    private ProjectData Project => App.LeditorInstance.Project;
    
    public void OpenEmptyUnit(string id)
    {
        MapData mapData = new MapData(id);
        
        UnitData unitData = new UnitData(id, id, UnitData.NO_OVERRIDE);
        
        Project.AddMap(mapData);
        Project.AddUnit(unitData);
    }
    
    public void SwitchTo(string id)
    {
        UnitData unit = App.LeditorInstance.ProjectEnvironment.GetUnit(id);
        MapData data = App.LeditorInstance.ProjectEnvironment.GetMap(unit.MapID);
        
        if (App.LeditorInstance.OpenBuffer(unit.MapID, out GridBuffer buffer))
        {
            buffer.AddAbove(data);           
        }

        buffer.FocusOnBufferCenter();
    }
}