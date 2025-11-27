namespace leditor.root;

public class UnitSwitcher
{
    private ProjectData Project => App.LeditorInstance.Project;
    
    public void OpenEmptyUnit(string id)
    {
        TilemapData tilemapData = new TilemapData(id);
        
        UnitData unitData = new UnitData(id, id, UnitData.NO_OVERRIDE);
        
        Project.AddMap(tilemapData);
        Project.AddUnit(unitData);
    }
    
    public void SwitchTo(string id)
    {
        UnitData unit = Project.GetUnit(id);
        TilemapData data = App.LeditorInstance.Project.GetMap(unit.MapID);
        
        if (App.LeditorInstance.OpenBuffer(unit.MapID, out GridBuffer buffer))
        {
            buffer.AddAbove(data);           
        }

        buffer.FocusOnBufferCenter();
    }
}