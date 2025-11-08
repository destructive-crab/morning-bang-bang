using deGUISpace;

namespace leditor.root.deGUILeditor;

public class UnitButton : GUIButton
{
    public string UnitID;
    public UnitSwitch UnitSwitch;

    public UnitButton(string label, RectGUIArea guiArea) : base(label, guiArea) { }

    public void ApplyData(string unitId, UnitSwitch unitSwitch)
    {
        UnitID = unitId;
        UnitSwitch = unitSwitch;
    }

   public override void LeftMouseButtonPress()
   {
      Color = PressedColor;
   }

    public override void OnLeftClick()
    {
        base.OnLeftClick();
        UnitSwitch.SwitchTo(UnitID);
    }
}