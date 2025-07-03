using banging_code.ui.main_menu;
using MothDIed;
using MothDIed.DI;

namespace banging_code
{
    public class BangBoot : GameStartPoint
    {
        public Coin coinPrefab;
        public GameStartArgs Args;
        
        public override IDependenciesProvider[] GetProviders()
        {
            return new[] { new CoinPrefabProvider(coinPrefab) };
        }

        protected override void StartGame()
        {
            TODO();
        }

        private async void TODO()
        {
            await Game.StartGame(Args);
            Game.SceneSwitcher.SwitchTo(new MainMenuScene());
        }
    }
}