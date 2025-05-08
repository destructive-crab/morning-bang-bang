using banging_code.common.extensions__for_gameobjects_;

namespace banging_code.ai.systems
{
    public class BasicEntityAnimator : AnimatorSystem
    {
        public override bool EnableOnStart() => true;

        
        public void PlayIdle()
        {
            Play("Idle");
        }

        public void PlayRun()
        {
            Play("Run");
        }

        public void PlayAttack()
        {
            Play("Attack");
        }
    }
}