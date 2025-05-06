using System.Collections;
using banging_code.health;
using MothDIed.DI;
using MothDIed.MonoSystems;
using UnityEngine;

namespace content.commuter_basement_I.entities.bastard
{
    public class BastardAttackSystem : MonoSystem
    {
        [Inject] private BastardAnimator animator;
        [Inject] private BastardBat bat;

        public override bool EnableOnStart()
        {
            return false;
        }

        public override void Enable()
        {
            base.Enable();

            Owner.StartCoroutine(TryAttack());
        }

        public IEnumerator TryAttack()
        {
            while (Enabled)
            {
                bat.Enable();
                animator.PlayAttack();
                        
                yield return new WaitForSeconds(1);    
            }
        }
    }
}