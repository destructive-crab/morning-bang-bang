using banging_code.dev;
using banging_code.level;
using banging_code.level.rooms;
using MothDIed.DI;
using UnityEngine;

namespace content.commuter_basement_II
{
    public class CommuterBasementII : CommuterBasement
    {
        public override string GetSceneName()
        {
            return nameof(CommuterBasementII);
        }

        protected override IDependenciesProvider GetProviders()
        {
            return new DevelopmentLevelProvider();
        }

        protected override BasicLevelConfig GetConfig()
        {
            return Resources.Load<BasicLevelConfigSO>("Levels/CommuterBasementIIConfig").Data;
        }
    }
}