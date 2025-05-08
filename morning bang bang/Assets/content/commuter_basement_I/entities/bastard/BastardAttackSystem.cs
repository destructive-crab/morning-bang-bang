using System.Collections;
using banging_code.ai.pathfinding;
using banging_code.ai.systems;
using banging_code.common;
using MothDIed.DI;
using MothDIed.MonoSystems;
using UnityEngine;

namespace content.commuter_basement_I.entities.bastard
{
    public class BastardAttackSystem : MonoSystem, IEntityAISystem
    {
        [Inject] private BastardAnimator animator;
        [Inject] private BastardBat bat;
        [Inject] private AttackTarget attackTarget;
        [Inject] private Flipper flipper;

        public override bool EnableOnStart()
        {
            return true;
        }

        public override void Enable()
        {
            base.Enable();

            Owner.StartCoroutine(TryAttack());
        }

        public IEnumerator TryAttack()
        {
            while (Enabled && Owner.isActiveAndEnabled)
            {
                while(attackTarget.Target == null) yield return new WaitForEndOfFrame();
                
                animator.PlayAttack();
                bat.Enable();
                flipper.FaceToTransform(attackTarget.Target.transform);
                
                yield return new WaitForSeconds(1);    
            }
        }
    }
}