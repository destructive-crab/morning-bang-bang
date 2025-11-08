using deGUISpace;

namespace leditor.root.deGUILeditor;

public class TileButton : GUIButton
{
   private PaintTool paintTool;
   public string tileID;

   public TileButton(string label, RectGUIArea guiArea) : base(label, guiArea)
   {
   }

   public void ApplyTool(string tileID, PaintTool tool)
   {
      paintTool = tool;
      this.tileID = tileID;
   }

   public override void OnLeftClick()
   {
      base.OnLeftClick();
      paintTool.SelectTile(tileID);
   }

   public override void LeftMouseButtonPress() => Color = PressedColor;
}