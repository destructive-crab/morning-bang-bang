using banging_code;
using banging_code.health;
using MothDIed.DI;

namespace content.commuter_basement_I.entities.bastard
{
    public class BastardBat : Trigger<HitableBody>
    {
        [Inject] private HitsHandler hitsHandler;

        public void Enable()
        {
            TriggerCollider().enabled = true;
        }

        private void Disable()
        {
            TriggerCollider().enabled = false;
        }
        
        private void Start() => OnEnter += Hit;

        private void Hit(HitableBody hitableBody)
        {
            hitsHandler.Hit(new DumbHitData(2), hitableBody);
            Disable();
        }
    }
}