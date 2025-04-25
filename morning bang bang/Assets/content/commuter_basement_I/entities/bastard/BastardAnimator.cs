using MothDIed.DI;
using MothDIed.ExtensionSystem;
using UnityEngine;

namespace content.commuter_basement_I.entities.bastard
{
    public class BastardAnimator : Extension
    {
        [Inject] private Animator animator;

        public void PlayIdle()
        {
            animator.Play("Idle");
        }

        public void PlayRun()
        {
            animator.Play("Run");
        }

        public void PlayAttack()
        {
            animator.Play("Attack");
        }
    }
}