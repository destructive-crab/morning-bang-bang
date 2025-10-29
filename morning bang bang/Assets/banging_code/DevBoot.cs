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
        public Coin coinPrefab;

        public override bool AllowDebug()
        {
            throw new System.NotImplementedException();
        }

        public override IDependenciesProvider[] GetProviders()
        {
            return new[] { new CoinPrefabProvider(coinPrefab) };
        }

        protected override UniTask Prepare()
        {
            InputService.Setup();
            
            Game.G<RunSystem>().StartDevRun();
            return UniTask.CompletedTask;
        }
    }
}