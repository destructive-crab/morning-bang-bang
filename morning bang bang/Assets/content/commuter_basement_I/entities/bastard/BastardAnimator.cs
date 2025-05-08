using banging_code.ai.pathfinding;
using banging_code.ai.systems;
using MothDIed.DI;
using UnityEngine;

namespace content.commuter_basement_I.entities.bastard
{
    public class BastardAnimator : BasicEntityAnimator
    {
        [Inject] private PathfinderTarget pathfinderTarget;
       
        [Inject]
        private void GetAnimator(Animator animator)
        {
            this.animator = animator;
        }
        public override void Update()
        {
            if(pathfinderTarget.Target != null) PlayRun();
        }
        
    }
}