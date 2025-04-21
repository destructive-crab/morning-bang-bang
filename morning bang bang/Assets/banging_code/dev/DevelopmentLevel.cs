using banging_code.camera_logic;
using banging_code.common;
using banging_code.health;
using banging_code.items;
using banging_code.level;
using banging_code.level.random_gen;
using banging_code.common.rooms;
using banging_code.player_logic.rat;
using MothDIed.DI;
using MothDIed.InputsHandling;
using UnityEngine;

namespace banging_code.dev
{
    public class DevelopmentLevel : LevelScene
    {
        public override string GetSceneName() => nameof(DevelopmentLevel);

        protected override void SetupModules()
        { 
            Modules.AddModule(new SceneDependenciesModule(new DevelopmentLevelProvider()));
            
            Modules.AddModule(new FabricAutoInjectModule());
            Modules.AddModule(new SceneAutoInjectModule());
            
            Modules.AddModule(new CCamera());
        }

        protected override void OnSceneLoaded()
        {
            //01. DO NOT START GAME INTERACTIONS AND GAME LOGIC
            
            //2. collect level data(rooms, enemies etc)
            BasicLevelConfig config = Resources.Load<BasicLevelConfigSO>("Dev/Test Level Config").Data;

            //3. generate level
            Generator generator = new BasicGenerator(config, this);

            generator.Generate();

            //4. spawn all enemies, npcs etc
            //5. spawn player
            PlayerInstance = SpawnPlayerInstance();
            Modules.Get<CCamera>().SetTarget(PlayerInstance.transform);

            ItemPickUp testItem = new GameObject().AddComponent<ItemPickUp>();
            testItem.FromNew("Gun");
            
            //6. spawn all ui
            //7. activate enemies
            //8. activate player
            PlayerInstance.Activate();
            
            //9. activate controls for player
            InputService.EnterPlayerMode();
        }

        protected override void PrepareLevel()
        {
            throw new System.NotImplementedException();
        }

        protected override void GenerateLevelBase()
        {
            throw new System.NotImplementedException();
        }

        protected override void ProcessGeneratedLevelSpawnContent()
        {
            throw new System.NotImplementedException();
        }

        protected override void SpawnPlayer()
        {
            
        }

        public override void UpdateScene()
        {
            Modules.UpdateModules();
        }

        private RatPlayer SpawnPlayerInstance()
        {
            Transform playerStuffContainer = new GameObject(G_O_NAMES.PLYR_STUFF).transform;

#if UNITY_EDITOR
            playerStuffContainer.SetAsFirstSibling();     
#endif
            
            return this.Fabric.Instantiate(GetPlayerPrefab(), GameObject.Find("[PLAYER START POINT]").transform.position, playerStuffContainer); 
        }

        protected override void SpawnUI()
        {
            throw new System.NotImplementedException();
        }

        protected override void FinishLevelSetup()
        {
            throw new System.NotImplementedException();
        }

        private RatPlayer GetPlayerPrefab() => Resources.Load<RatPlayer>("Dev/Dev Rat Prefab");
    }

    public class DevelopmentLevelProvider : IDependenciesProvider
    {
        [Provide(true)]
        public HitsHandler ProvideHitsHandler() => new HitsHandler();
    }
}