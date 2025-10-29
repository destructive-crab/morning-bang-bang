using banging_code.debug;
using banging_code.pause;
using banging_code.runs_system;
using banging_code.settings;
using banging_code.ui.main_menu;
using Cysharp.Threading.Tasks;
using MothDIed;
using MothDIed.InputsHandling;

namespace banging_code
{
    public class BangBoot : GameStartPoint
    {
        public DebuggerConfig Config;
        private GameSettings settings;
        
        public override bool AllowDebug()
        {
            return settings.Data.EnableDebugFeatures;
        }

        protected override async UniTask Prepare()
        {
            InputService.Setup();
            settings = new GameSettings();
            
            await settings.LoadFromFile();
        }

        public override UniTask BuildModules(GMModulesStorage modulesStorage)
        {
            base.BuildModules(modulesStorage);
            
            modulesStorage.AutoRegister<GameSettings>(settings);
            modulesStorage.AutoRegister<BangDebugger>(new BangDebugger(Config));
            modulesStorage.AutoRegister<RunSystem>(new RunSystem());
            modulesStorage.AutoRegister<PauseSystem>(new PauseSystem());
            
            return UniTask.CompletedTask;
        }

        public override void Complete()
        {
            base.Complete();
            Game.G<SceneSwitcher>().SwitchTo(new MainMenuScene());
        }
    }
}