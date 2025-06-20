using banging_code.ai.common;
using banging_code.ai.pathfinding;
using banging_code.ai.systems;
using MothDIed.DI;
using UnityEngine;

namespace content.commuter_basement_I.entities.bastard
{
    public class BastardAnimator : BasicEntityAnimator
    {
        [Inject] private PathfinderTarget pathfinderTarget;
        [Inject] private AttackTarget attackTarget;
        
        private SlayEffect slayEffect;

        [Inject]
        private void GetAnimator(Animator animator, SlayEffect slayEffect)
        {
            this.animator = animator;
            this.slayEffect = slayEffect;
        }
        
        public override void Update()
        {
            if(attackTarget.Target == null && pathfinderTarget.Target != null) PlayRun();
        }

        public override void PlayAttack()
        {
            base.PlayAttack();
            slayEffect.PlayDefault();
        }
    }
}