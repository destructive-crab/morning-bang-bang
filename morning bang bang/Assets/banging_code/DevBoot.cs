using System.Threading.Tasks;
using banging_code.dev;
using MothDIed;
using MothDIed.DI;
using MothDIed.InputsHandling;

namespace banging_code
{
    public class DevBoot : GameStartPoint
    {
        public Coin coinPrefab;
        
        public override IDependenciesProvider[] GetProviders()
        {
            return new[] { new CoinPrefabProvider(coinPrefab) };
        }

        protected override void StartGame()
        {
            InputService.Setup();
            
            Game.RunSystem.StartDevRun();
        }
    }
}