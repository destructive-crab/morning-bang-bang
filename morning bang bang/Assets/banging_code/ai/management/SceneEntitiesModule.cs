using System.Collections.Generic;
using MothDIed.Core.GameObjects;
using MothDIed.Scenes;
using MothDIed.Scenes.SceneModules;
using UnityEngine;

namespace banging_code.ai.pathfinding
{
    public class SceneEntitiesModule : SceneModule
    {
        private readonly List<Entity> entities = new();

        public override void PrepareModule(Scene scene)
        {
        }

        public override void UpdateModule(Scene scene)
        {
            foreach (var entity in entities)
            {
                if (entity.IsSleeping) entity.Tick();
            }
        }

        private void AddEntities(Entity[] entities)
        {
            this.entities.AddRange(entities);
        }
        private void RemoveEntities(Entity[] entities)
        {
            foreach (var entity in entities)
            {
                this.entities.Remove(entity);
            }
        }

        public void GoSleepAll()
        {
            foreach (var entity in entities)
            {
                entity.GoSleep();
            }
        }

        public void InitializeAll()
        {
            foreach (var entity in entities)
            {
                if(entity == null)  {Debug.Log("AAAAA"); continue;}
                entity.Initialize();
            }
        }

        public void WakeUpAll()
        {
            foreach (var entity in entities)
            {
                entity.WakeUp();
            }
        }

        public class EntityFabricModule : GameFabricSceneModule
        {
            private readonly SceneEntitiesModule module;

            public EntityFabricModule(SceneEntitiesModule module)
            {
                this.module = module;
            }

            public override void OnGameObjectInstantiated(GameObject instance)
            {
                var entities = instance.GetComponentsInChildren<Entity>();
                
                if (entities.Length > 0)
                {
                    module.AddEntities(entities);
                }
            }
            public override void BeforeGameObjectDestroyed(GameObject instance)
            {
                var entities = instance.GetComponentsInChildren<Entity>();
                
                if (entities.Length  > 0)
                {
                    module.RemoveEntities(entities);
                }
            }
        }
    }
}