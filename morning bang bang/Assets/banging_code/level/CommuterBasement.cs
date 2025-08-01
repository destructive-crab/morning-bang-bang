using banging_code.ai;
using banging_code.ai.pathfinding;
using banging_code.common;
using banging_code.camera_logic;
using banging_code.items;
using banging_code.level.random_gen;
using banging_code.common.rooms;
using banging_code.level.entity_locating;
using banging_code.level.light;
using banging_code.level.structure.map;
using banging_code.pause;
using banging_code.player_logic.rat;
using banging_code.ui;
using MothDIed.DI;
using MothDIed.InputsHandling;
using UnityEngine;

namespace banging_code.level
{
    public abstract class CommuterBasement : LevelScene
    {
        protected abstract IDependenciesProvider GetProviders();
        protected abstract BasicLevelConfig GetConfig();
        
        protected override void SetupModules()
        { 
            Modules.AddModule(new SceneDependenciesModule(GetProviders()));

            Modules.AddModule(new FabricAutoInjectModule());
            Modules.AddModule(new SceneAutoInjectModule());
            Modules.AddModule(new ScenePauseModule());

            Hierarchy = Modules.AddModule(new LevelHierarchyModule());
            Modules.AddModule(new CCamera());
            Map = new TilemapBasedLevelMap(Modules.Get<LevelHierarchyModule>());
            Modules.AddModule(new LevelMapCallbacksGameFabricModule(this));
            EntitiesController = Modules.AddModule(new SceneEntitiesModule());
            Modules.AddModule(new SceneEntitiesModule.EntityFabricModule(EntitiesController));
            Modules.AddModule(Map);
            Modules.AddModule(new LightManager());
            Modules.AddModule(new LocationManager(this));
            Modules.AddModule(new EntityPathfindingSystem(Map));
        }
        protected override void PrepareLevel() { }
        protected override void GenerateLevelBase()
        {
            //2. collect level data(rooms, enemies etc)
            BasicLevelConfig config = GetConfig(); 

            //3. generate level
            Modules.Get<LevelHierarchyModule>().SetupGeneratedLevelBase();
            Generator = new BasicGenerator(config, this);

            Hierarchy.Rooms = Generator.Generate();
            Map.Refresh();
        }

        protected override void ProcessGeneratedLevelSpawnContent()
        {
            foreach (Room room in Hierarchy.Rooms)
            {
                Modules.Get<LocationManager>().RegisterLocation(room.RoomID);
                
                var spawners = room.GetComponentsInChildren<EntitySpawner>();
                
                foreach (var spawner in spawners)
                {
                    var enemies = spawner.Spawn();
                    
                    foreach (var monoEntity in enemies)
                    {
                        Modules.Get<LocationManager>().AddEntity(monoEntity, room.RoomID);
                    }
                }
            }
        }

        protected override void SpawnPlayer()
        {
            PlayerInstance = SpawnPlayerInstance();
            Modules.Get<CCamera>().SetTarget(PlayerInstance.transform);
        }

        protected override void SpawnUI()
        {
            var hudPrefab = Resources.Load<HUD>(PTH.HUDPrefab);
            Fabric.Instantiate(hudPrefab, Vector2.zero);
        }

        protected override void FinishLevelSetup()
        {
            //7. activate enemies
            EntitiesController.InitializeAll();
            EntitiesController.GoSleepAll();
            
            //8. activate player
            PlayerInstance.Activate();
            
            //9. activate controls for player
            InputService.SwitchTo(InputService.Mode.Player);
            
            Modules.Get<CCamera>().EnterChillCamera();
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

        private RatPlayer GetPlayerPrefab() => Resources.Load<RatPlayer>("Dev/Dev Rat Prefab");
    }
}