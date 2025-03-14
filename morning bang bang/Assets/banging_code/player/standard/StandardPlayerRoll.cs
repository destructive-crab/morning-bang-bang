using System;
using System.Collections;
using banging_code.player_logic.states;
using MothDIed.DI;
using UnityEngine;

namespace banging_code.player_logic.standard
{
    public class StandardPlayerRoll : PlayerRoll
    {
        public override bool AllowRepeats { get; } = false;
        public override Type[] CanBeEnteredOnlyFrom => new[] {typeof(PlayerMove)};

        private const float RollTime = 0.1f;
        
        [Inject] private PlayerAnimator animator;
        [Inject] private Rigidbody2D rigidbody;
        
        public override void Enter(PlayerRoot playerRoot)
        {
            animator.PlayRoll(1/RollTime);
            playerRoot.StartCoroutine(DashCoroutine());
        }
        
        private IEnumerator DashCoroutine()
        {
            StartDash();
            {
                Vector2 velocity = rigidbody.linearVelocity * 0.3f;
                var dashStopTime = Time.time + RollTime;
                
                while (Time.time < dashStopTime)
                {
                    var position = rigidbody.position;

                    rigidbody.MovePosition
                            (Vector2.MoveTowards(position, position + velocity, Time.deltaTime * 30));
                    
                    yield return new WaitForFixedUpdate();
                }
            }
            rigidbody.linearVelocity = Vector2.zero;

            yield return ReleaseDash(rigidbody);
        }
        
        private void StartDash()
        {
            InProcess = true;
        }

        private IEnumerator ReleaseDash(Rigidbody2D rigidbody2D)
        {
            yield return new WaitForSeconds(0.15f);
            InProcess = false;
        }
    }
}