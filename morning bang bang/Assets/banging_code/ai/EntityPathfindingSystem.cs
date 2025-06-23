using System.Collections.Generic;
using banging_code.ai.pathfinding;
using banging_code.ai.systems;
using Cysharp.Threading.Tasks;
using MothDIed;
using MothDIed.Scenes;
using MothDIed.Scenes.SceneModules;

namespace banging_code.ai
{
    public class EntityPathfindingSystem : SceneModule
    {
        private readonly List<MonoEntity> monoEntities = new();
        private readonly LevelMap map;

        public EntityPathfindingSystem(LevelMap map)
        {
            this.map = map;
        }

        public void AddActiveEntities(MonoEntity[] entities)
        {
            monoEntities.AddRange(entities);
        }

        public override void StartModule(Scene scene)
        {
            base.StartModule(scene);
            UpdatePath();
        }

        private async void UpdatePath()
        {
            while (Game.RunSystem.IsInRun)
            {
                while (monoEntities.Count == 0)
                {
                    await UniTask.Delay(100);
                }
                for (var i = 0; i < monoEntities.Count; i++)
                {
                    var monoEntity = monoEntities[i];
                
                    //is enemy alive
                    if (monoEntity == null)
                    {
                        monoEntities.RemoveAt(i);
                        i--;
                        continue;
                    }
                
                    //update entity path
                    if (monoEntity.Data.TryGet<PathfinderTarget>(out var target) && target != null)
                    {
                        monoEntity.Systems.Get<PathUpdater>().UpdatePath();
                    }

                    await UniTask.Delay(100);
                }
            }
          
        }
        
        public override void UpdateModule(Scene scene)
        {
            base.UpdateModule(scene);


        }
    }
}