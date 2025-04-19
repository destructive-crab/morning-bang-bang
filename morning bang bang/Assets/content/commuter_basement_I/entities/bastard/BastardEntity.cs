using System.Collections.Generic;
using banging_code.ai;
using banging_code.ai.pathfinding;
using banging_code.ai.targeting;
using UnityEngine;

namespace content.commuter_basement_I.entities.bastard
{
    public class BastardEntity : Entity
    {
        private Pathfinder pathfinder;
        public TargetToEntities currentTarget { get; private set; }
        
        protected override IEnumerable<RequireIn> Require()
        {
            List<RequireIn> structure = new List<RequireIn>();

            RequireIn collider = new();
            collider
                .PathToObject("Triggers/FieldOfView")
                .WithComponents(true, typeof(EntityFieldOfView))
                .ForEachComponent((components => {
                    foreach (var component in components)
                    {
                        if (component is EntityFieldOfView fov)
                        {
                            fov.SetRadius(5);
                        }
                    }
                }))
                .Final((colliderObject) =>
                {
                    Debug.Log("lol kek aboba");
                });
            
            structure.Add(collider);

            return structure;
        }

        public override void GoSleep()
        {
        }

        public override void WakeUp()
        {
            
        }

        public override void Tick()
        {
            
        }
    }
}