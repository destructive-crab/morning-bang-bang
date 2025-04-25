using System;

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
                case DumbHitData dumbHit:
                    who.TakeDumbHit(dumbHit);
                    break;
            }
            
            OnHit?.Invoke(who, from);
        } 
    }
}