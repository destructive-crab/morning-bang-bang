using UnityEngine;

namespace banging_code.health
{
    public abstract class HitableBody : MonoBehaviour
    {
        public abstract void TakeBulletHit(BulletHitData data);
        public abstract void TakeStabHit(StabHitData data);
    }
}