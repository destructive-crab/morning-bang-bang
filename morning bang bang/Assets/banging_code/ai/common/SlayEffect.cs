using UnityEngine;

namespace banging_code.ai.common
{
    [RequireComponent(typeof(Animator))]
    public class SlayEffect : MonoBehaviour
    {
        private Animator animator;

        private void Start()
        {
            animator = GetComponent<Animator>();
        }

        public void PlayDefault()
        {
            animator.Play("AttackEffectDefault");
        }
    }
}