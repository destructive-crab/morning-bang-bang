using System;
using banging_code.health;
using UnityEngine;

namespace banging_code.ai
{
    [RequireComponent(typeof(Entity))]
    public class EntityHealth : HitableBody
    {
        [field: SerializeField] public int Health { get; private set; }
        public event Action<Entity> OnDie;

        public void TakeHit(HitData data)
        {
            Health -= data.DamageAmount;
            
            if(Health <= 0) OnDie?.Invoke(GetComponent<Entity>());
        }
        
        public override void TakeBulletHit(BulletHitData data)
        {
            TakeHit(data);
        }

        public override void TakeStabHit(StabHitData data)
        {
            TakeHit(data);
        }

        public override void TakeDumbHit(DumbHitData data)
        {
            TakeHit(data);
        }
    }
}