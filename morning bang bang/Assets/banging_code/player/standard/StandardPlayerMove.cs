using System;
using banging_code.common;
using banging_code.player_logic.states;
using MothDIed.DI;
using MothDIed.InputsHandling;
using UnityEngine;

namespace banging_code.player_logic.standard
{
    public class StandardPlayerMove : PlayerMove
    {
        public override Type[] CanBeEnteredOnlyFrom => Array.Empty<Type>();
        public override Type[] CannotBeEnteredFrom => Array.Empty<Type>();
        public override bool AllowRepeats => false;

        [Inject] private PlayerAnimator animator;
        [Inject] private Rigidbody2D rigidbody2D;
        
        public override void FixedUpdate(PlayerRoot playerRoot)
        {
            animator.PlayRun(1);
            rigidbody2D.linearVelocity = InputService.Movement * 4;
            Movement = InputService.Movement;

            if (Movement.x > 0) playerRoot.Direction = GameDirection.Right;
            else if (Movement.x < 0) playerRoot.Direction = GameDirection.Left;
            
            if (Movement.y > 0) playerRoot.Direction = GameDirection.Top;
            else if (Movement.y < 0) playerRoot.Direction = GameDirection.Bottom;
        }
    }
}