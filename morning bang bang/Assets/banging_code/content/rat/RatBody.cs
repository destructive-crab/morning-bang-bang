using banging_code.common;
using banging_code.health;
using banging_code.runs_system;
using MothDIed;

namespace banging_code.player_logic.rat
{
    public class RatBody : HitableBody
    {
        private void TakeHit(HitData data)
        {
            if (Game.G<RunSystem>().Data.PlayerHealth.Remove(data.DamageAmount, 0))
            {
                //disable gui
                //disable plauer
                //spawn deadbody
                //change camera
                //enable death gui 
                
                Game.G<RunSystem>().Die();
            }
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

        public override ID EntityID { get; }
    }
}