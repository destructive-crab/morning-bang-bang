using banging_code.common.extensions__for_gameobjects_;

namespace banging_code.ai.systems
{
    public class BasicEntityAnimator : AnimatorSystem
    {
        public override bool EnableOnStart() => true;

        public virtual void PlayIdle()
        {
            Play("Idle");
        }

        public virtual void PlayRun()
        {
            Play("Run");
        }

        public virtual void PlayAttack()
        {
            Play("Attack");
        }
    }
}