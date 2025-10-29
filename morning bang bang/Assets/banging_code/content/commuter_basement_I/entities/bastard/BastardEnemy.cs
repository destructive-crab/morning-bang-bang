using banging_code.ai;
using banging_code.ai.common;
using banging_code.ai.pathfinding;
using banging_code.ai.systems;
using banging_code.ai.targeting;
using banging_code.common;
using banging_code.runs_system;
using MothDIed;
using UnityEngine;

namespace content.commuter_basement_I.entities.bastard
{
    public class BastardEnemy : Enemy
    {
        private EntityAttackRange attackRange;
        private EntityFieldOfView fov;

        protected override string GetEntityPrefix() => "bastard_";

        public override void Initialize()
        {
            Systems.SetOwner(this);
            
            Game.G<RunSystem>().Data.Level.Map.NewDynamicObstacle(gameObject.AddComponent<DynamicObstacle>());

            Data.Register(new EntityPath());
            Data.Register(new PathfinderTarget());
            Data.Register(new AttackTarget());

            CachedComponents.Register(GetComponent<Rigidbody2D>());
            CachedComponents.Register(GetComponentInChildren<Animator>());
            CachedComponents.Register(GetComponentInChildren<SpriteRenderer>());
            attackRange = GetComponentInChildren<EntityAttackRange>();
            CachedComponents.Register(attackRange);
            CachedComponents.Register(GetComponentInChildren<BastardBat>());
            CachedComponents.Register(GetComponent<EntityHealth>());
            CachedComponents.Register(GetComponentInChildren<SlayEffect>());
            
            Health = CachedComponents.Get<EntityHealth>();

            Systems.AddSystem(new Flipper(true));
            Systems.AddSystem(new BastardAnimator());
            Systems.AddSystem(new TargetSelector());
            Systems.AddSystem(new PathUpdater());
            Systems.AddSystem(new BasicMovementSystem());
            Systems.AddSystem(new BastardAttackSystem());

            Systems.StartContainer();
            Health.OnDie += Die;
        }

        public override void WakeUp()
        {
            base.WakeUp();
        }

        public override void GoSleep()
        {
            base.GoSleep();
        }

        public override void Tick()
        {
            UpdateData();
            Do();
        }

        private void UpdateData()
        {
            Data.Get<PathfinderTarget>().Target = Systems.Get<TargetSelector>().BestTarget;
//           Data.Get<AttackTarget>().Target = attackRange.Best;
        }
        
        private void Do() { }
    }
}