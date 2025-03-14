using System;
using UnityEngine;

namespace banging_code.health
{
    public class HitsHandler
    {
        public event Action<HitableBody, HitData> OnHit;
        
        public void Hit(HitData from, HitableBody who)
        {
            if(from == null || who == null)
                return;
            
            switch (from)
            {
                case BulletHitData bullet:
                    who.TakeBulletHit(bullet);
                    break;
                case StabHitData stabHitData:
                    who.TakeStabHit(stabHitData);
                    break;
            }
            
            Debug.Log(who.gameObject.name + " was hit by " + from.GetType().ToString() + " on damage points " + from.DamageAmount);
            OnHit?.Invoke(who, from);
        } 
    }
}