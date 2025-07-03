using MothDIed.GUI;
using MothDIed.Scenes;

namespace banging_code.ui.main_menu
{
    public class MainMenuScene : Scene
    {
        public override string GetSceneName()
        {
            return "Main Menu";
        }

        protected override void SetupModules()
        {
            Modules.AddModule(new SceneGUIModule(true));
        }
    }
}