using deGUISpace;
using leditor.root.deGUILeditor;

namespace leditor.root;

public class UnitSwitch
{
    private ProjectData Project => App.LeditorInstance.project;

    private GUIGroup group;
    
    public UnitSwitch()
    {
    }

    public void BuildGUI()
    {
        group = new GUIGroup(new RectGUIArea(Anchor.CenterTop, 0, 0, -1, 30));
        for (var i = 0; i < Project.Units.Length; i++)
        {
            UnitData unit = Project.Units[i];
            
            int x = 120;
            int y = 20;

            UnitButton button = new UnitButton(unit.UnitID, new RectGUIArea(Anchor.LeftTop, i * x, 0, x, y));
            button.ApplyData(unit.UnitID, this);

            group.AddChild(button);
        }
        
        deGUI.PushGUIElement(group);
    }
    
    public void SwitchTo(string id)
    {
        UnitData unit = Project.GetUnit(id);
        TilemapData data = App.LeditorInstance.project.GetMap(unit.MapID);
        
        App.LeditorInstance.buffer.Clear();
        App.LeditorInstance.buffer.AddAbove(data);
        App.LeditorInstance.buffer.AddAbove(data);

        App.LeditorInstance.FocusOnBufferCenter();
    }
}