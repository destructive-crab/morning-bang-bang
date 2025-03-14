using banging_code.health;
using MothDIed.DI;
using UnityEngine;

namespace banging_code.dev
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class HitOnTrigger : Trigger
    {
        [Inject] private HitsHandler hitsHandler;
        private void Start() => OnEnter += Hit;
        private void Hit(Collider2D entered)
        {
            HitableBody body = entered.GetComponent<HitableBody>();
            //Проверка на null уже реализована в Hit()
            hitsHandler.Hit(new StabHitData(1), body);
        }
    }
}