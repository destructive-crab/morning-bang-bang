using MothDIed.DI;

namespace banging_code
{
    public class CoinPrefabProvider : IDependenciesProvider
    {
        private readonly Coin coinPrefab;

        public CoinPrefabProvider(Coin coinPrefab)
        {
            this.coinPrefab = coinPrefab;
        }

        [Provide(true)]
        Coin ProvideCoinPrefab() => coinPrefab;
    }
}