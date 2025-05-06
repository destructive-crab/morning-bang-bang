using banging_code.health;
using banging_code.level;
using banging_code.common.rooms;
using MothDIed.DI;
using UnityEngine;

namespace banging_code.dev
{
    public class DevelopmentLevel : CommuterBasement 
    {
        public override string GetSceneName()
        {
            return "DevelopmentLevel";
        }

        protected override IDependenciesProvider GetProviders()
        {
            return new DevelopmentLevelProvider();
        }

        protected override BasicLevelConfig GetConfig()
        {
            return Resources.Load<BasicLevelConfigSO>("Dev/Test Level Config").Data;
        }
    }

    public class DevelopmentLevelProvider : IDependenciesProvider
    {
        [Provide(true)]
        public HitsHandler ProvideHitsHandler() => new HitsHandler();
    }
}