using System.Collections;
using banging_code.ai;
using banging_code.ai.pathfinding;
using banging_code.ai.targeting;
using banging_code.common;
using UnityEngine;

namespace content.commuter_basement_I.entities.bastard
{
    public class BastardEntity : Entity
    {
        private Pathfinder pathfinder;
        public TargetToEntities currentTarget { get; private set; }

        private EntityAttackRange attackTrigger;
        public bool CanMove = true;

        public override void GoSleep()
        {
            
        }

        public override void Initialize()
        {
            Extensions.SetOwner(this);

            CachedComponents.Register(GetComponent<Rigidbody2D>());
            CachedComponents.Register(GetComponentInChildren<Animator>());
            CachedComponents.Register(GetComponentInChildren<SpriteRenderer>());
            CachedComponents.Register(GetComponentInChildren<EntityFieldOfView>());
            CachedComponents.Register(GetComponentInChildren<EntityAttackRange>());
            CachedComponents.Register(GetComponentInChildren<BastardBat>());
            CachedComponents.Register(GetComponent<EntityHealth>());
            Health = CachedComponents.Get<EntityHealth>();

            Extensions.AddExtension(new Flipper(true));
            Extensions.AddExtension(new BastardAnimator());
            Extensions.AddExtension(new TargetSelector());
            Extensions.AddExtension(new BasicMovementSystem());
            Extensions.AddExtension(new BastardAttackSystem());
            
            Extensions.StartContainer();
            Health.OnDie += Die;
        }

        public override void WakeUp()
        {
            Extensions.Get<TargetSelector>().OnBestTargetChanged += (_) =>
            {
                StartMoving();
            };
            CachedComponents.Get<EntityAttackRange>().OnEnter += (other) =>
            {
                if (other == Extensions.Get<TargetSelector>().BestTarget)
                {
                    StartCoroutine(Attack());
                }
            };
        }

        private void StartMoving()
        {
            if(!CanMove) return;
            
            Extensions.Get<BastardAnimator>().PlayRun();
            Extensions.Get<BasicMovementSystem>().StartMoving();
        }

        private IEnumerator Attack()
        {
            CanMove = false;
            Extensions.Get<BasicMovementSystem>().StopMoving();
            yield return StartCoroutine(Extensions.Get<BastardAttackSystem>().TryAttack());
            CanMove = true;
            StartMoving();
        }

        public override void Tick()
        {
            
        }
    }
}