using banging_code.ai.pathfinding;
using MothDIed.Core.GameObjects;
using UnityEngine;

namespace banging_code.level.structure.map
{
    public class LevelMapCallbacksGameFabricModule : GameFabricSceneModule
    {
        public LevelScene level;

        public LevelMapCallbacksGameFabricModule(LevelScene level)
        {
            this.level = level;
        }

        public override void OnInstantiated<TObject>(TObject instance)
        {
            if (instance is GameObject gameObject)
            {
                var dyOb = gameObject.GetComponentInChildren<LevelMap.DynamicObstacle>();

                if (dyOb != null)
                {
                    level.Map.NewDynamicObstacle(dyOb);
                }
            }
        }

        public override void OnDestroyed(Object toDestroy)
        {
            if (toDestroy is GameObject gameObject)
            {
                 var dyOb = gameObject.GetComponentInChildren<LevelMap.DynamicObstacle>();
 
                 if (dyOb != null)
                 {
                     level.Map.ClearDyObListFromNulls();
                 }               
            }
        }
    }
}