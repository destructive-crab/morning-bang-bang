using deGUISpace;

namespace leditor.root.deGUILeditor;

public class ToolButton : GUIButton
{
   public Tool tool;
   public Toolset toolset;

   public ToolButton(string label, RectGUIArea guiArea) : base(label, guiArea)
   {
   }

   public void ApplyTool(Tool tool, Toolset toolset)
   {
      this.tool = tool;
      this.toolset = toolset;
   }

   public override void LeftMouseButtonPress()
   {
      Color = PressedColor;
   }

   public override void OnLeftClick()
   {
      base.OnLeftClick();
      toolset.SelectTool(tool);
   }
}