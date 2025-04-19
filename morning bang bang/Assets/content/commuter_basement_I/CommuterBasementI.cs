using banging_code.dev;
using banging_code.level;
using banging_code.common.rooms;
using content.commuter_basement_I.entities.bastard;
using MothDIed.DI;
using UnityEngine;

namespace content.commuter_basement_I
{
    public class CommuterBasementI : CommuterBasement
    {
        public override string GetSceneName() => nameof(CommuterBasementI);

        protected override IDependenciesProvider GetProviders()
        {
            return new DevelopmentLevelProvider();
        }

        protected override BasicLevelConfig GetConfig()
        {
            return Resources.Load<BasicLevelConfigSO>("Levels/CommuterBasementIConfig").Data;
        }

        protected override void OnSceneLoaded()
        {
            base.OnSceneLoaded();

            var bastard = new GameObject("bastard", typeof(BastardEntity));
        }
    }
}