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
        MapData data = App.LeditorInstance.ProjectEnvironment.GetMap(id);
        
        if (App.LeditorInstance.OpenBuffer(id, out GridBuffer buffer))
        {
            buffer.AddAbove(data);           
        }

        buffer.FocusOnBufferCenter();
    }
}