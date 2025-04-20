using banging_code.health;
using MothDIed.DI;
using UnityEngine;

namespace banging_code.dev
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class HitOnTrigger : Trigger<HitableBody>
    {
        [Inject] private HitsHandler hitsHandler;
        private void Start() => OnEnter += Hit;
        private void Hit(HitableBody body)
        {
            hitsHandler.Hit(new StabHitData(1), body);
        }
    }
}