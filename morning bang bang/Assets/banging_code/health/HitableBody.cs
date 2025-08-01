using MothDIed;
using UnityEngine;

namespace banging_code.health
{
    public abstract class HitableBody : MonoEntity 
    {
        public abstract void TakeBulletHit(BulletHitData data);
        public abstract void TakeStabHit(StabHitData data);
        public abstract void TakeDumbHit(DumbHitData dumbHitData);
    }
}