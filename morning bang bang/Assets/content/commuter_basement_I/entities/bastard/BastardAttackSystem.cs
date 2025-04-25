using System.Collections;
using banging_code.health;
using MothDIed.DI;
using MothDIed.ExtensionSystem;
using UnityEngine;

namespace content.commuter_basement_I.entities.bastard
{
    public class BastardAttackSystem : Extension
    {
        [Inject] private BastardAnimator animator;
        [Inject] private BastardBat bat;
        
        public IEnumerator TryAttack()
        {
            bat.Enable();
            animator.PlayAttack();
            
            yield return new WaitForSeconds(1);
        }
    }
}