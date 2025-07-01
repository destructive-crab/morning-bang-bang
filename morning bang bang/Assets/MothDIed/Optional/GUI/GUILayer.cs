using banging_code.debug;

namespace MothDIed.GUI
{
    public abstract class GUILayer : GUIElement
    {
        public override void MoveUnder(GUIElement guiElement)
        {
            LGR.PW($"You tried to set parent to GUILayer. Parent -> {guiElement.name}; Layer -> {this.name}");
        }
    }
}