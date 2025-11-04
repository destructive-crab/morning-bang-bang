using System.Threading.Tasks;
using banging_code.dev;
using banging_code.runs_system;
using Cysharp.Threading.Tasks;
using MothDIed;
using MothDIed.DI;
using MothDIed.InputsHandling;

namespace banging_code
{
    public class DevBoot : GameStartPoint
    {
        public override bool AllowDebug()
        {
            return true;
        }

        public override UniTask BuildModules(GMModulesStorage modulesStorage)
        {
            base.BuildModules(modulesStorage);
            modulesStorage.AutoRegister<DragonBonesMothDIedModule>(new DragonBonesMothDIedModule());
            return UniTask.CompletedTask;
        }

        protected override UniTask Prepare()
        {
            InputService.Setup();

            
            return UniTask.CompletedTask;
        }

        public override void Complete()
        {
            base.Complete();
            
            Game.G<SceneSwitcher>().SwitchTo(new CommonScene("TEST"));
        }
    }
}