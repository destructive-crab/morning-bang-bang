using MothDIed;
using MothDIed.DI;
using MothDIed.InputsHandling;

namespace banging_code
{
    public class BangBoot : GameStartPoint
    {
        public Coin coinPrefab;
        
        public override IDependenciesProvider[] GetProviders()
        {
            return new[] { new CoinPrefabProvider(coinPrefab) };
        }

        protected override void StartGame()
        {
            InputService.Initialize();
            
            Game.InnerLoop();
            Game.SwitchTo(new CommonScene("Menu"));
        }
    }
}