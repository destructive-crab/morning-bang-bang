using System;
using banging_code.player_logic.states;
using MothDIed.DI;
using UnityEngine;

namespace banging_code.player_logic.standard
{
    public class StandardPlayerIdle : PlayerIdle
    {
        public override Type[] CanBeEnteredOnlyFrom => Array.Empty<Type>();
        public override Type[] CannotBeEnteredFrom => Array.Empty<Type>();
        public override bool AllowRepeats { get; } = false;

        [Inject] private PlayerAnimator playerAnimator;
        [Inject] private Rigidbody2D rigidbody;
        
        public override void Enter(PlayerRoot playerRoot)
        {
            playerAnimator.PlayIdle(1);
            rigidbody.linearVelocity = Vector2.zero;
        }
    }
}