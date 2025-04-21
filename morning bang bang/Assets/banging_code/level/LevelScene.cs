using banging_code.ai.pathfinding;
using banging_code.level.random_gen;
using banging_code.player_logic;
using MothDIed.Scenes;

namespace banging_code.level
{
    public abstract class LevelScene : Scene
    {
        public PlayerRoot PlayerInstance { get; protected set; }
        public LevelMap Map { get; protected set; }
        public LevelHierarchyModule Hierarchy { get; protected set; }
        public SceneEntitiesModule EntitiesController { get; protected set; }
        
        protected Generator Generator;

        protected override void OnSceneLoaded()
        {
            PrepareLevel();
            GenerateLevelBase();
            ProcessGeneratedLevelSpawnContent();
            SpawnPlayer();
            SpawnUI();
            FinishLevelSetup();
        }

        protected abstract void PrepareLevel();
        protected abstract void GenerateLevelBase();
        protected abstract void ProcessGeneratedLevelSpawnContent();
        protected abstract void SpawnPlayer();
        protected abstract void SpawnUI();
        protected abstract void FinishLevelSetup();
    }
}