using UnityEngine;

namespace banging_code.player_logic.states
{
    public abstract class PlayerMove : PlayerState
    {
        public Vector2 Movement { get; protected set; }
    }
}