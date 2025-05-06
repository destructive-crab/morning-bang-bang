using System.Collections;
using banging_code.ai;
using banging_code.ai.pathfinding;
using banging_code.ai.systems;
using banging_code.ai.targeting;
using banging_code.common;
using UnityEngine;

namespace content.commuter_basement_I.entities.bastard
{
    public class BastardEntity : Entity
    {
        private EntityAttackRange attackRange;
        private EntityFieldOfView fov;

        public override void Initialize()
        {
            Systems.SetOwner(this);

            Data.Register(new EntityPath());
            Data.Register(new PathfinderTarget());
            Data.Register(new AttackTarget());

            CachedComponents.Register(GetComponent<Rigidbody2D>());
            CachedComponents.Register(GetComponentInChildren<Animator>());
            CachedComponents.Register(GetComponentInChildren<SpriteRenderer>());
            CachedComponents.Register(GetComponentInChildren<EntityFieldOfView>());
            attackRange = GetComponentInChildren<EntityAttackRange>();
            CachedComponents.Register(attackRange);
            CachedComponents.Register(GetComponentInChildren<BastardBat>());
            CachedComponents.Register(GetComponent<EntityHealth>());
            
            Health = CachedComponents.Get<EntityHealth>();

            Systems.AddSystem(new Flipper(true));
            Systems.AddSystem(new BastardAnimator());
            Systems.AddSystem(new TargetSelector());
            Systems.AddSystem(new BasicMovementSystem());
            Systems.AddSystem(new BastardAttackSystem());
            Systems.AddSystem(new PathUpdater());
            
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
            Data.Get<AttackTarget>().Target = attackRange.Best;
        }


        private void Do()
        {
            if (Data.Get<AttackTarget>().Target != null && !Systems.Get<BastardAttackSystem>().Enabled)
            {
                Data.Get<EntityPath>().Path = null;
                Systems.Get<BastardAttackSystem>().Enable();
                Systems.Get<PathUpdater>().Disable();
            }
            else if (Data.Get<PathfinderTarget>().Target != null && !Systems.Get<PathUpdater>().Enabled)
            {
                Systems.Get<PathUpdater>().Enable();
                Systems.Get<BastardAttackSystem>().Disable();
            }
        }
    }
}