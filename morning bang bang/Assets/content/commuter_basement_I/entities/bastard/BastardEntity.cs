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

        public override void GoSleep()
        {
        }

        public override void Initialize()
        {
            Extensions.SetOwner(this);
            
            CachedComponents.Register(GetComponentInChildren<EntityFieldOfView>());
            CachedComponents.Register(GetComponentInChildren<EntityAttackRange>());
            CachedComponents.Register(GetComponent<Rigidbody2D>());
            
            Extensions.AddExtension(new TargetSelector());
            Extensions.AddExtension(new BasicEntityToTargetMovement());
            
            Extensions.StartContainer();
        }

        public override void WakeUp()
        {
            Debug.Log("WAKE UP");
            Extensions.Get<TargetSelector>().OnBestTargetChanged += (_) =>
            {
                Debug.Log("BEST TARGET CHANGED");
                Extensions.Get<BasicEntityToTargetMovement>().StartMoving();
            };
        }

        public override void Tick()
        {
        }
    }
}