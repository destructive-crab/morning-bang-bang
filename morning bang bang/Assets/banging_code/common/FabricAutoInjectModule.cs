using MothDIed;
using MothDIed.Core.GameObjects;

namespace banging_code
{
    public sealed class FabricAutoInjectModule : GameFabricSceneModule
    {
        public override void OnInstantiated<TObject>(TObject instance)
        {
            Game.DIKernel.InjectWithBase(instance);
        }
    }
}